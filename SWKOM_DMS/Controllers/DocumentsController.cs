using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SWKOM_DMS.DTOs;
using SWKOM_DMS.Entities;
using SWKOM_DMS.Services; 
using System.Threading.Tasks;
using SWKOM_DMS.logging;

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

        // Constructor now includes RabbitMQ service
        public DocumentsController(IMapper mapper, ILoggerWrapper logger, IDocumentRepository repository, RabbitMQService rabbitMqService)
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


        //[HttpPost("upload")]
        //public async Task<IActionResult> UploadDocument([FromBody] DocumentDto documentDto)
        //{
        //    if (documentDto == null)
        //    {
        //        _logger.Warn("Upload attempted with a null DocumentDto.");
        //        return BadRequest("Document data is missing.");
        //    }

        //    try
        //    {
        //        // Map DTO to Document entity
        //        var documentEntity = _mapper.Map<Document>(documentDto);

        //        // Save the document in the database
        //        await _repository.AddDocumentAsync(documentEntity);
        //        _logger.Info($"Document saved to the database successfully: {documentEntity.FileName}");

        //        // Send message to RabbitMQ
        //        _rabbitMqService.SendMessage($"Document uploaded: {documentEntity.FileName}");
        //        _logger.Info($"Message sent to RabbitMQ for document: {documentEntity.FileName}");

        //        return Ok("Document uploaded and message sent to RabbitMQ successfully.");
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.Error("Error occurred while uploading document or sending message to RabbitMQ.", ex);
        //        return StatusCode(500, "An error occurred while uploading the document.");
        //    }
        //}

        [HttpPost("upload")]
        public async Task<IActionResult> UploadDocument([FromBody] DocumentDto documentDto)
        {
            if (documentDto == null)
            {
                _logger.Warn("Upload attempted with a null DocumentDto.");
                return BadRequest("Document data is missing.");
            }

            try
            {
                // Save file to the shared directory
                var sharedDirectory = Path.Combine(Directory.GetCurrentDirectory(), "SharedDirectory");
                if (!Directory.Exists(sharedDirectory))
                {
                    Directory.CreateDirectory(sharedDirectory);
                }

                var filePath = Path.Combine(sharedDirectory, documentDto.FileName);
                var fileBytes = Convert.FromBase64String(documentDto.FileContent);
                await System.IO.File.WriteAllBytesAsync(filePath, fileBytes);
                documentDto.FilePath = filePath;

                // Map DTO to Document entity
                var documentEntity = _mapper.Map<Document>(documentDto);

                // Save the document in the database
                await _repository.AddDocumentAsync(documentEntity);
                _logger.Info($"Document saved to the database successfully: {documentEntity.FileName}");

                // Send message to RabbitMQ
                _rabbitMqService.SendMessage(documentDto.FilePath); // Send full file path
                _logger.Info($"Message sent to RabbitMQ for document: {documentEntity.FilePath}");

                return Ok("Document uploaded and message sent to RabbitMQ successfully.");
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
