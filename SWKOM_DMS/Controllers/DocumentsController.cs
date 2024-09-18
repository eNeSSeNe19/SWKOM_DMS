using Microsoft.AspNetCore.Mvc;

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

        public DocumentsController(ILogger<DocumentsController> logger)
        {
            _logger = logger;
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
        public IActionResult UploadDocument()
        {
            // In Sprint 1, this just returns a hardcoded response
            return Ok("Document uploaded successfully!");
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

            // Returning hardcoded search results
            return Ok(results);
        }

        // 5. Delete a document by ID (hardcoded response)
        [HttpDelete("{id}")]
        public IActionResult DeleteDocument(int id)
        {
            if (id < 0 || id >= Documents.Length)
            {
                return NotFound("Document not found");
            }

            // Simulating document deletion
            return Ok($"Document with ID {id} deleted successfully.");
        }
    }
}
