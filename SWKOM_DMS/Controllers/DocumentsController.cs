using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SWKOM_DMS.DTOs;
using SWKOM_DMS.Entities;
using SWKOM_DMS.Services; // Add this to include RabbitMQ service
using System.Threading.Tasks;

namespace SWKOM_DMS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentsController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ILogger<DocumentsController> _logger;
        private readonly IDocumentRepository _repository;
        private readonly RabbitMQService _rabbitMqService; // Inject RabbitMQService

        // Constructor now includes RabbitMQ service
        public DocumentsController(IMapper mapper, ILogger<DocumentsController> logger, IDocumentRepository repository, RabbitMQService rabbitMqService)
        {
            _mapper = mapper;
            _logger = logger;
            _repository = repository;
            _rabbitMqService = rabbitMqService;
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

        // 2. Upload a document using the DTO and map to entity
        [HttpPost("upload")]
        public async Task<IActionResult> UploadDocument([FromBody] DocumentDto documentDto)
        {
            if (documentDto == null)
            {
                return BadRequest("Document data is missing.");
            }

            // Map DTO to Document entity
            var documentEntity = _mapper.Map<Document>(documentDto);

            // Save the document in the database
            await _repository.AddDocumentAsync(documentEntity);

            // Send message to RabbitMQ
            _rabbitMqService.SendMessage($"Document uploaded: {documentEntity.FileName}");

            return Ok("Document uploaded and message sent to RabbitMQ successfully.");
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
