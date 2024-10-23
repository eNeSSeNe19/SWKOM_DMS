using Microsoft.AspNetCore.Mvc;
<<<<<<< Updated upstream
=======
using SWKOM_DMS.DTOs;
using SWKOM_DMS.Entities;
using SWKOM_DMS.Services; // Add this to include RabbitMQ service
using System.Threading.Tasks;
>>>>>>> Stashed changes

namespace SWKOM_DMS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentsController : ControllerBase
    {
        private static readonly string[] Documents = new[]
        {
            "Document1.pdf", "Document2.docx", "Document3.xlsx"
        };

        private readonly ILogger<DocumentsController> _logger;
<<<<<<< Updated upstream

        public DocumentsController(ILogger<DocumentsController> logger)
=======
        private readonly IDocumentRepository _repository;
        private readonly RabbitMQService _rabbitMqService; // Inject RabbitMQService

        // Constructor now includes RabbitMQ service
        public DocumentsController(IMapper mapper, ILogger<DocumentsController> logger, IDocumentRepository repository, RabbitMQService rabbitMqService)
>>>>>>> Stashed changes
        {
            _logger = logger;
<<<<<<< Updated upstream
=======
            _repository = repository;
            _rabbitMqService = rabbitMqService;
>>>>>>> Stashed changes
        }

        // 1. Get list of documents (hardcoded for now)

        [HttpGet("/")]
        public IActionResult GetRoot()
        {
            // Return a hardcoded response when accessing the root URL
            return Ok(new { message = "Welcome to the Document Management API", documents = Documents });
        }

        [HttpGet("list")]
        public IActionResult GetDocuments()
        {
            // Returning hardcoded list of documents
            return Ok(Documents);
        }

        // 2. Get a single document by ID (hardcoded response)
        [HttpGet("{id}")]
        public IActionResult GetDocumentById(int id)
        {
            if (id < 0 || id >= Documents.Length)
            {
                return NotFound("Document not found");
            }

            // Returning a hardcoded single document
            return Ok(new { id = id, name = Documents[id], content = "This is the content of the document." });
        }

        // 3. Upload a document (simulating upload for now)
        [HttpPost("upload")]
<<<<<<< Updated upstream
        public IActionResult UploadDocument()
        {
            // In Sprint 1, this just returns a hardcoded response
            return Ok("Document uploaded successfully!");
=======
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
>>>>>>> Stashed changes
        }

        // 4. Search for documents by a keyword (hardcoded)
        [HttpGet("search/{keyword}")]
        public IActionResult SearchDocuments(string keyword)
        {
            var results = Documents.Where(d => d.Contains(keyword)).ToList();
            if (!results.Any())
            {
                return NotFound("No documents found with the given keyword.");
            }

<<<<<<< Updated upstream
            // Returning hardcoded search results
            return Ok(results);
        }

        // 5. Delete a document by ID (hardcoded response)
        [HttpDelete("{id}")]
        public IActionResult DeleteDocument(int id)
        {
            if (id < 0 || id >= Documents.Length)
=======
                await _repository.AddDocumentAsync(document); // Using repository to add document

                // Send test message to RabbitMQ
                _rabbitMqService.SendMessage($"Test document uploaded: {document.FileName}");

                return Ok("Database connection and insert operation successful.");
            }
            catch (Exception ex)
>>>>>>> Stashed changes
            {
                return NotFound("Document not found");
            }

            // Simulating document deletion
            return Ok($"Document with ID {id} deleted successfully.");
        }
    }
}
