using Elastic.Clients.Elasticsearch;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.IO;
using System.Text;
using Tesseract;
using log4net;
using System.Diagnostics;

class OCRWorker
{
    private static readonly ILog _logger = LogManager.GetLogger(typeof(OCRWorker));
    private static ElasticsearchClient _elasticClient;

    public static void Main(string[] args)
    {
        // Configure Log4Net
        log4net.Config.XmlConfigurator.Configure(new FileInfo("log4net.config"));
        _logger.Info("OCR Worker started...");

        // Initialize Elasticsearch client
        InitializeElasticsearch();

        // Ensure TESSDATA_PREFIX is set properly
        var tessdataPath = Path.Combine(Directory.GetCurrentDirectory(), "tessdata");
        if (!Directory.Exists(tessdataPath))
        {
            _logger.Fatal($"Tesseract data path not found: {tessdataPath}");
            return;
        }
        Environment.SetEnvironmentVariable("TESSDATA_PREFIX", tessdataPath);
        _logger.Info($"TESSDATA_PREFIX set to: {tessdataPath}");

        var sharedDirectory = Path.Combine(Directory.GetCurrentDirectory(), "SharedDirectory");
        if (!Directory.Exists(sharedDirectory))
        {
            _logger.Fatal($"Shared directory not found: {sharedDirectory}");
            return;
        }

        var factory = new ConnectionFactory()
        {
            HostName = "rabbitmq",
            UserName = "guest",
            Password = "guest"
        };

        IConnection connection = null;
        IModel channel = null;

        // Retry mechanism to connect to RabbitMQ
        for (int i = 0; i < 5; i++)
        {
            try
            {
                _logger.Info($"Attempting to connect to RabbitMQ... Attempt {i + 1}/5");
                connection = factory.CreateConnection();
                channel = connection.CreateModel();
                _logger.Info("Connected to RabbitMQ successfully.");
                break;
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to connect to RabbitMQ. Retrying...", ex);
                System.Threading.Thread.Sleep(5000);
            }
        }

        if (connection == null || channel == null)
        {
            _logger.Fatal("Failed to connect to RabbitMQ after multiple attempts. Exiting...");
            return;
        }

        // Declare queue
        channel.QueueDeclare(queue: "documents_queue",
                             durable: false,
                             exclusive: false,
                             autoDelete: false,
                             arguments: null);

        _logger.Info("Waiting for messages...");

        // Create a consumer to receive messages
        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var filePath = Encoding.UTF8.GetString(body);
            _logger.Info($"Received file path from RabbitMQ: {filePath}");

            if (!File.Exists(filePath))
            {
                _logger.Error($"File not found: {filePath}");
                return;
            }

            // Process OCR
            var ocrResult = ProcessDocument(filePath);
            _logger.Info($"OCR Result: {ocrResult}");

            // Index the result in Elasticsearch
            IndexResultInElasticsearch(filePath, ocrResult);
        };

        channel.BasicConsume(queue: "documents_queue",
                             autoAck: true,
                             consumer: consumer);

        while (true)
        {
            System.Threading.Thread.Sleep(Timeout.Infinite);
        }
    }

    private static void InitializeElasticsearch()
    {
        var settings = new ElasticsearchClientSettings(new Uri("http://elasticsearch:9200"));
        _elasticClient = new ElasticsearchClient(settings);
        _logger.Info("Initialized Elasticsearch client.");
    }

    private static string ProcessDocument(string filePath)
    {
        try
        {
            string result = string.Empty;

            // Validate that the file exists
            if (!File.Exists(filePath))
            {
                _logger.Error($"File does not exist: {filePath}");
                return result;
            }

            // Check if the file is a PDF
            if (Path.GetExtension(filePath).ToLower() == ".pdf")
            {
                _logger.Info($"Detected PDF file: {filePath}. Converting to images...");
                var imageDir = ConvertPdfToImage(filePath);

                // Check if PDF conversion succeeded
                if (string.IsNullOrEmpty(imageDir) || !Directory.Exists(imageDir))
                {
                    _logger.Error($"PDF-to-image conversion failed for file: {filePath}");
                    return result;
                }

                var imageFiles = Directory.GetFiles(imageDir, "*.png");
                if (imageFiles.Length == 0)
                {
                    _logger.Error($"No images found after converting PDF: {filePath}");
                    return result;
                }

                // Perform OCR on each image file
                foreach (var imageFile in imageFiles)
                {
                    try
                    {
                        _logger.Info($"Performing OCR on image: {imageFile}");
                        result += PerformOcr(imageFile);
                    }
                    catch (Exception ocrEx)
                    {
                        _logger.Error($"Error performing OCR on image: {imageFile}", ocrEx);
                    }
                }
            }
            else
            {
                // Directly process image files
                _logger.Info($"Processing image file: {filePath}");
                result = PerformOcr(filePath);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.Error($"Unexpected error during OCR processing for file: {filePath}", ex);
            return string.Empty;
        }
    }


    private static string PerformOcr(string imagePath)
    {
        using var engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default);
        using var img = Pix.LoadFromFile(imagePath);
        using var page = engine.Process(img);

        return page.GetText();
    }

    private static string ConvertPdfToImage(string pdfPath)
    {
        var outputDir = Path.Combine(Path.GetDirectoryName(pdfPath), "images");
        if (!Directory.Exists(outputDir))
        {
            Directory.CreateDirectory(outputDir);
        }

        var outputImagePath = Path.Combine(outputDir, "page-%03d.png"); // Ghostscript outputs one image per page

        var gsArguments = $"-sDEVICE=png16m -r300 -dNOPAUSE -dBATCH -sOutputFile=\"{outputImagePath}\" \"{pdfPath}\"";


        var processInfo = new ProcessStartInfo
        {
            FileName = "gs",
            Arguments = gsArguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (var process = new Process { StartInfo = processInfo })
        {
            process.Start();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                throw new IOException($"Ghostscript failed to convert PDF to images: {process.StandardError.ReadToEnd()}");
            }
        }

        return outputDir;
    }

    private static void IndexResultInElasticsearch(string filePath, string ocrResult)
    {
        try
        {
            var document = new
            {
                FilePath = filePath,
                OCRContent = ocrResult,
                ProcessedAt = DateTime.UtcNow
            };


            var response = _elasticClient.Index(document, i => i.Index("ocr_results"));
            if (response.IsValidResponse)
            {
                _logger.Info($"Successfully indexed OCR result for {filePath}");
            }
            else
            {
                _logger.Error($"Failed to index OCR result: {response.DebugInformation}");
            }
        }
        catch (Exception ex)
        {
            _logger.Error("Error while indexing result in Elasticsearch", ex);
        }
    }
}
