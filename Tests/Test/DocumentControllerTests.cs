using Moq;
using NUnit.Framework;
using SWKOM_DMS.Controllers;
using SWKOM_DMS.Entities;
using SWKOM_DMS.DTOs;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SWKOM_DMS.Tests
{
    [TestFixture]
    public class DocumentControllerTests
    {
        private Mock<IDocumentRepository> _mockRepo;
        private Mock<IMapper> _mockMapper;
        private Mock<ILogger<DocumentsController>> _mockLogger;
        private DocumentsController _controller;

        [SetUp]
        public void SetUp()
        {
            _mockRepo = new Mock<IDocumentRepository>();
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILogger<DocumentsController>>();
            _controller = new DocumentsController(_mockMapper.Object, _mockLogger.Object, _mockRepo.Object);
        }

        [Test]
        public async Task UploadDocument_Returns_OkResult_When_Valid()
        {
            // Arrange
            var documentDto = new DocumentDto
            {
                FileName = "test.pdf",
                ContentType = "application/pdf",
                FileContent = new byte[] { 0x1, 0x2, 0x3 }
            };

            _mockMapper.Setup(m => m.Map<Document>(It.IsAny<DocumentDto>())).Returns(new Document());

            // Act
            var result = _controller.UploadDocument(documentDto);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [Test]
        public async Task UploadDocument_Returns_BadRequest_When_DocumentDto_Is_Null()
        {
            // Act
            var result = _controller.UploadDocument(null);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        [Test]
        public async Task TestDbConnection_ShouldCallAddDocumentAsync()
        {
            // Arrange
            var testDocument = new Document
            {
                FileName = "Test Document",
                FileType = "pdf",
                FileSize = 1000,
                ContentType = "application/pdf",
                FileContent = new byte[] { 0x1, 0x2, 0x3 },
                UploadDate = DateTime.Now
            };

            // Act
            var result = await _controller.TestDbConnection();

            // Assert
            _mockRepo.Verify(r => r.AddDocumentAsync(It.Is<Document>(d =>
                d.FileName == testDocument.FileName &&
                d.FileType == testDocument.FileType &&
                d.ContentType == testDocument.ContentType &&
                d.FileSize == testDocument.FileSize
            )), Times.Once);

            // Optionally, you can also check the response
            Assert.IsInstanceOf<OkObjectResult>(result);
        }



    }
}
