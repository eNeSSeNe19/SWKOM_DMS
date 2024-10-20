using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SWKOM_DMS.DTOs;
using SWKOM_DMS.Entities;
using System.Threading.Tasks;

namespace SWKOM_DMS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentsController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ILogger<DocumentsController> _logger;
        private readonly IDocumentRepository _repository; // Injecting repository

        // Constructor now includes repository
        public DocumentsController(IMapper mapper, ILogger<DocumentsController> logger, IDocumentRepository repository)
        {
            _mapper = mapper;
            _logger = logger;
            _repository = repository;
        }

        // 3. Upload a document using the DTO and map to entity
        [HttpPost("upload")]
        public IActionResult UploadDocument([FromBody] DocumentDto documentDto)
        {
            if (documentDto == null)
            {
                return BadRequest("Document data is missing.");
            }

            // Map DTO to Document entity
            var documentEntity = _mapper.Map<Document>(documentDto);

            // Normally, you'd save the entity to the database here
            // For now, we return a success response
            return Ok("Document uploaded successfully.");
        }

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
                    UploadDate = DateTime.Now
                };

                await _repository.AddDocumentAsync(document); // Using repository to add document
                return Ok("Database connection and insert operation successful.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Database connection failed: {ex.Message}");
            }
        }
    }
}
