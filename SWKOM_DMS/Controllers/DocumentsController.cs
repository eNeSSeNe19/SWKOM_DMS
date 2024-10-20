using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SWKOM_DMS.DTOs;
using SWKOM_DMS.Entities;

namespace SWKOM_DMS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentsController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ILogger<DocumentsController> _logger;

        public DocumentsController(IMapper mapper, ILogger<DocumentsController> logger)
        {
            _mapper = mapper;
            _logger = logger;
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
    }
}
