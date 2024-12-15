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
        private readonly IRabbitMQService _rabbitMqService; 
        private readonly DocumentDbContext _dbContext;
        private readonly ElasticsearchClient _elasticClient; 

        public DocumentsController(
            IMapper mapper,
            ILoggerWrapper logger,
            IDocumentRepository repository,
            IRabbitMQService rabbitMqService, 
            DocumentDbContext dbContext,
            IElasticsearchClientProvider elasticClientProvider)
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
                _logger.Warn("No file uploaded.");
                return BadRequest("No file uploaded.");
            }

            try
            {
                _logger.Info("Starting file upload process.");

                var sharedDirectory = Path.Combine(Directory.GetCurrentDirectory(), "SharedDirectory");
                if (!Directory.Exists(sharedDirectory))
                {
                    _logger.Warn("Shared directory does not exist. Creating directory.");
                    Directory.CreateDirectory(sharedDirectory);
                }

                var filePath = Path.Combine(sharedDirectory, file.FileName);
                _logger.Info($"Saving file to: {filePath}");

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                _logger.Info("File saved successfully. Creating document entity.");

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

                _logger.Info("Adding document to database.");
                _dbContext.Documents.Add(document);
                await _dbContext.SaveChangesAsync();

                _logger.Info("Document saved in database. Sending message to RabbitMQ.");
                _rabbitMqService.SendMessage(filePath);

                _logger.Info("Message sent to RabbitMQ successfully.");
                return Ok("File uploaded successfully, and message sent to RabbitMQ.");
            }
            catch (Exception ex)
            {
                _logger.Error($"Error occurred during upload process: {ex.Message}", ex);
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
                    .Index("ocr_results") // Ensure this is the correct index
                    .Query(q => q
                        .Match(m => m
                            .Field("ocrContent") // Field name should match the indexed data
                            .Query(query)
                        )
                    )
                );

                if (!searchResponse.IsValidResponse)
                {
                    return StatusCode(500, $"Elasticsearch search failed: {searchResponse.ElasticsearchServerError?.Error.Reason}");
                }

                var results = searchResponse.Documents; // Retrieve matching documents
                return Ok(results);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error occurred during search: {ex.Message}");
            }
        }

        // Delete document from the database and return confirmation
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDocument(int id)
        {
            // Find the document in the database
            var document = await _dbContext.Documents.FindAsync(id);
            if (document == null)
            {
                return NotFound("Document not found.");
            }

            try
            {
                // Remove the document file from the shared directory
                var sharedDirectory = Path.Combine(Directory.GetCurrentDirectory(), "SharedDirectory");
                var filePath = Path.Combine(sharedDirectory, document.FileName);

                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath); // Delete the file from the filesystem
                    _logger.Info($"File deleted from shared directory: {filePath}");
                }
                else
                {
                    _logger.Warn($"File not found in shared directory: {filePath}");
                }

                // Remove the document from the database
                _dbContext.Documents.Remove(document);
                await _dbContext.SaveChangesAsync();

                return Ok("Document and associated file deleted successfully.");
            }
            catch (Exception ex)
            {
                _logger.Error("Error occurred while deleting the document or its file.", ex);
                return StatusCode(500, $"Error deleting document: {ex.Message}");
            }
        }

    }
}
