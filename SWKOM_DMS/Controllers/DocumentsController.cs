using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SWKOM_DMS.DTOs;
using SWKOM_DMS.Entities;
using SWKOM_DMS.Services;
using System.Threading.Tasks;
using SWKOM_DMS.logging;
using Microsoft.EntityFrameworkCore;
using Elastic.Clients.Elasticsearch;

namespace SWKOM_DMS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentsController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ILoggerWrapper _logger;
        private readonly IDocumentRepository _repository;
        private readonly RabbitMQService _rabbitMqService;
        private readonly DocumentDbContext _dbContext;
        private readonly ElasticsearchClient _elasticClient; // Using your custom Elasticsearch client

        public DocumentsController(
            IMapper mapper,
            ILoggerWrapper logger,
            IDocumentRepository repository,
            RabbitMQService rabbitMqService,
            DocumentDbContext dbContext,
            ElasticsearchClientProvider elasticClientProvider)
        {
            _mapper = mapper;
            _logger = logger;
            _repository = repository;
            _rabbitMqService = rabbitMqService;
            _dbContext = dbContext;
            _elasticClient = elasticClientProvider.GetClient(); // Obtain Elasticsearch client from provider
        }

        // 1. List all documents
        [HttpGet("list")]
        public async Task<IActionResult> GetDocuments()
        {
            try
            {
                var documents = await _repository.GetAllDocumentsAsync(); // Fetch all documents from the repository
                return Ok(documents);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error fetching documents: {ex.Message}");
            }
        }

        // 2. Upload document and send it to RabbitMQ for OCR processing
        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            try
            {
                var sharedDirectory = Path.Combine(Directory.GetCurrentDirectory(), "SharedDirectory");
                if (!Directory.Exists(sharedDirectory))
                {
                    Directory.CreateDirectory(sharedDirectory);
                }

                var filePath = Path.Combine(sharedDirectory, file.FileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                var document = new Document
                {
                    FileName = file.FileName,
                    FileType = file.ContentType,
                    FileSize = file.Length,
                    UploadDate = DateTime.UtcNow,
                    FilePath = filePath,
                    FileContent = System.IO.File.ReadAllBytes(filePath),
                    ContentType = "pdf"
                };

                _dbContext.Documents.Add(document);
                await _dbContext.SaveChangesAsync();

                _rabbitMqService.SendMessage(filePath);
                _logger.Info($"Message sent to RabbitMQ for document: {document.FilePath}");

                return Ok("File uploaded successfully, and message sent to RabbitMQ.");
            }
            catch (Exception ex)
            {
                _logger.Error("Error occurred while uploading document or sending message to RabbitMQ.", ex);
                return StatusCode(500, "An error occurred while uploading the document.");
            }
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchDocuments([FromQuery] string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                return BadRequest("Query parameter is required.");
            }

            try
            {
                var searchResponse = await _elasticClient.SearchAsync<object>(s => s
                    .Index("ocr_results") // The name of the index
                    .Query(q => q
                        .Match(m => m
                            .Field("ocrContent") // The field containing OCR text
                            .Query(query) // The search term
                        )
                    )
                );

                if (!searchResponse.IsValidResponse)
                {
                    return StatusCode(500, $"Elasticsearch search failed: {searchResponse.ElasticsearchServerError?.Error.Reason}");
                }

                var results = searchResponse.Documents; // Get the results from the search
                return Ok(results);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error occurred during search: {ex.Message}");
            }
        }

    }
}
