using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Tesseract;
using log4net;

class OCRWorker
{
    private static readonly ILog _logger = LogManager.GetLogger(typeof(OCRWorker));

    public static void Main(string[] args)
    {
        // Configure Log4Net
        log4net.Config.XmlConfigurator.Configure(new FileInfo("log4net.config"));
        _logger.Info("OCR Worker started...");

        var factory = new ConnectionFactory()
        {
            HostName = "rabbitmq", // Ensure this matches the RabbitMQ service name in docker-compose.yml
            UserName = "guest",    // Update if custom credentials are used
            Password = "guest"     // Update if custom credentials are used
        };

        IConnection connection = null;
        IModel channel = null;

        // Retry mechanism to connect to RabbitMQ
        for (int i = 0; i < 5; i++) // Retry 5 times
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
                System.Threading.Thread.Sleep(5000); // Wait for 5 seconds before retrying
            }
        }

        // If unable to connect after retries, exit the application
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
            var message = Encoding.UTF8.GetString(body);
            _logger.Info($"Received message: {message}");

            // Fetch file path from message
            string filePath = message;

            // Simulate OCR Processing
            var ocrResult = ProcessDocument(filePath); // Update this to fetch the actual file path
            _logger.Info($"OCR Result: {ocrResult}");

            // Optionally send the result back to another queue
            SendMessageToQueue(channel, "ocr_results_queue", ocrResult);
        };

        channel.BasicConsume(queue: "documents_queue",
                             autoAck: true,
                             consumer: consumer);

        // Prevent the application from exiting
        while (true)
        {
            System.Threading.Thread.Sleep(Timeout.Infinite);
        }
    }

    private static string ProcessDocument(string filePath)
    {
        string outputDir = "/tmp/ocr-images";
        Directory.CreateDirectory(outputDir);

        try
        {
            // Step 1: Convert PDF to Images
            ConvertPdfToImages(filePath, outputDir);

            // Step 2: Perform OCR on each image
            StringBuilder ocrText = new StringBuilder();
            using var engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default);

            foreach (var imagePath in Directory.GetFiles(outputDir, "*.png"))
            {
                using var img = Pix.LoadFromFile(imagePath);
                using var page = engine.Process(img);
                ocrText.AppendLine(page.GetText());
            }

            // Cleanup temporary files
            Directory.Delete(outputDir, true);

            return ocrText.ToString();
        }
        catch (Exception ex)
        {
            _logger.Error("Error during OCR processing", ex);
            return string.Empty;
        }
    }

    private static void ConvertPdfToImages(string pdfPath, string outputDir)
    {
        try
        {
            _logger.Info($"Converting PDF to images: {pdfPath}");

            var ghostscriptPath = "/usr/bin/gs"; // Path to Ghostscript
            var args = $"-sDEVICE=pngalpha -o {outputDir}/output-%d.png -r300 \"{pdfPath}\"";

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = ghostscriptPath,
                    Arguments = args,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                string errorOutput = process.StandardError.ReadToEnd();
                _logger.Error($"Ghostscript failed: {errorOutput}");
                throw new Exception($"Ghostscript error: {errorOutput}");
            }

            _logger.Info("PDF successfully converted to images.");
        }
        catch (Exception ex)
        {
            _logger.Error("Error during PDF-to-image conversion", ex);
            throw;
        }
    }

    private static void SendMessageToQueue(IModel channel, string queueName, string message)
    {
        var body = Encoding.UTF8.GetBytes(message);
        channel.BasicPublish(exchange: "",
                             routingKey: queueName,
                             basicProperties: null,
                             body: body);
        _logger.Info($"Sent message to queue {queueName}: {message}");
    }
}
