using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SWKOM_DMS.DTOs;
using SWKOM_DMS.Entities;
using SWKOM_DMS.Services; 
using System.Threading.Tasks;
using SWKOM_DMS.logging;
using Microsoft.EntityFrameworkCore;

namespace SWKOM_DMS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentsController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ILoggerWrapper _logger;
        private readonly IDocumentRepository _repository;
        private readonly RabbitMQService _rabbitMqService; // Inject RabbitMQService
        private readonly DocumentDbContext _dbContext;

        // Constructor now includes RabbitMQ service
        public DocumentsController(IMapper mapper, ILoggerWrapper logger, IDocumentRepository repository, RabbitMQService rabbitMqService, DocumentDbContext dbContext)
        {
            _mapper = mapper;
            _logger = logger;
            _repository = repository;
            _rabbitMqService = rabbitMqService;
            _dbContext = dbContext;
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




        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            try
            {
                // Save file to shared directory
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

                // Create a document entity
                var document = new Document
                {
                    FileName = file.FileName,
                    FileType = file.ContentType,
                    FileSize = file.Length,
                    UploadDate = DateTime.UtcNow,
                    FilePath = filePath,
                    FileContent = System.IO.File.ReadAllBytes(filePath), // Read file content
                    ContentType = "pdf" // or dynamically determine
                };

                // Save document to database
                _dbContext.Documents.Add(document);
                await _dbContext.SaveChangesAsync();

                // Send message to RabbitMQ
                _rabbitMqService.SendMessage(filePath); // Provide full file path for processing
                _logger.Info($"Message sent to RabbitMQ for document: {document.FilePath}");

                return Ok("File uploaded successfully, and message sent to RabbitMQ.");
            }
            catch (Exception ex)
            {
                _logger.Error("Error occurred while uploading document or sending message to RabbitMQ.", ex);
                return StatusCode(500, "An error occurred while uploading the document.");
            }
        }







        // 3. Test database connection
        [HttpGet("test-db-connection")]
        public async Task<IActionResult> TestDbConnection()
        {
            try
            {
                var document = new Document
                {
                    FileName = "Test Document",
                    FileType = "pdf",
                    FileSize = 1000,
                    ContentType = "application/pdf",
                    FileContent = new byte[] { 0x1, 0x2, 0x3 },
                    UploadDate = DateTime.UtcNow
                };

                await _repository.AddDocumentAsync(document); // Using repository to add document

                // Send test message to RabbitMQ
                _rabbitMqService.SendMessage($"Test document uploaded: {document.FileName}");

                return Ok("Database connection and insert operation successful.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Database connection failed: {ex.Message}");
            }
        }
    }
}
