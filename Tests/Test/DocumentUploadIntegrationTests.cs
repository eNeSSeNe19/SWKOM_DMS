using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SWKOM_DMS.Controllers;
using SWKOM_DMS.Entities;
using SWKOM_DMS.Services;
using SWKOM_DMS.logging;
using Microsoft.EntityFrameworkCore;
using Elastic.Clients.Elasticsearch;
using System.Threading.Tasks;

namespace SWKOM_DMS.Tests
{
    [TestFixture]
    public class DocumentIntegrationTests
    {
        private DocumentsController _controller;
        private DocumentDbContext _dbContext;
        private ILoggerWrapper _logger;
        private Mock<IRabbitMQService> _mockRabbitMqService;
        private Mock<IElasticsearchClientProvider> _mockElasticProvider;
        private Mock<ElasticsearchClient> _mockElasticClient;

        [SetUp]
        public void SetUp()
        {
            // In-Memory Database Setup
            var options = new DbContextOptionsBuilder<DocumentDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;
            _dbContext = new DocumentDbContext(options);

            // Logger Setup
            _logger = LoggerFactory.GetLogger();

            // Mock RabbitMQ Service
            _mockRabbitMqService = new Mock<IRabbitMQService>();

            // Mock Elasticsearch Client and Provider
            _mockElasticClient = new Mock<ElasticsearchClient>();
            _mockElasticProvider = new Mock<IElasticsearchClientProvider>();
            _mockElasticProvider.Setup(p => p.GetClient()).Returns(_mockElasticClient.Object);

            // Initialize Controller
            _controller = new DocumentsController(
                null, // AutoMapper
                _logger,
                new DocumentRepository(_dbContext),
                _mockRabbitMqService.Object,
                _dbContext,
                _mockElasticProvider.Object
            );
        }

        [TearDown]
        public void TearDown()
        {
            _dbContext?.Dispose();
        }

        [Test]
        public async Task Upload_Returns_OkResult_WhenFileIsUploaded()
        {
            // Arrange
            var fileMock = new Mock<Microsoft.AspNetCore.Http.IFormFile>();
            var content = "Fake file content";
            var fileName = "test.pdf";
            var memoryStream = new System.IO.MemoryStream();
            var writer = new System.IO.StreamWriter(memoryStream);
            writer.Write(content);
            writer.Flush();
            memoryStream.Position = 0;

            fileMock.Setup(f => f.OpenReadStream()).Returns(memoryStream);
            fileMock.Setup(f => f.FileName).Returns(fileName);
            fileMock.Setup(f => f.Length).Returns(memoryStream.Length);
            fileMock.Setup(f => f.ContentType).Returns("application/pdf");

            // Act
            var result = await _controller.Upload(fileMock.Object);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [Test]
        public async Task Upload_Returns_BadRequest_WhenFileIsNull()
        {
            // Act
            var result = await _controller.Upload(null);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        [Test]
        public async Task Upload_Returns_BadRequest_WhenFileIsEmpty()
        {
            // Arrange
            var fileMock = new Mock<Microsoft.AspNetCore.Http.IFormFile>();
            fileMock.Setup(f => f.Length).Returns(0);

            // Act
            var result = await _controller.Upload(fileMock.Object);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        [Test]
        public async Task Upload_Returns_BadRequest_WhenFileIsNotPdf()
        {
            // Arrange
            var fileMock = new Mock<Microsoft.AspNetCore.Http.IFormFile>();
            fileMock.Setup(f => f.ContentType).Returns("image/png");

            // Act
            var result = await _controller.Upload(fileMock.Object);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        [Test]
        public async Task DeleteDocument_Returns_OkResult_And_RemovesDocumentFromDatabase()
        {
            // Arrange: Seed a document into the database with required properties
            var document = new Document
            {
                Id = 1,
                FileName = "test.pdf",
                FilePath = "SharedDirectory/test.pdf", // Required
                ContentType = "application/pdf", // Required
                FileContent = new byte[] { 0x01, 0x02, 0x03 }, // Required binary file content
                FileType = "pdf", // Required
                UploadDate = DateTime.UtcNow, // Not required but useful for completeness
                FileSize = 1234 // Optional but useful for testing
            };

            // Add the document to the in-memory database
            await _dbContext.Documents.AddAsync(document);
            await _dbContext.SaveChangesAsync();

            // Act: Call the Delete method
            var result = await _controller.DeleteDocument(document.Id);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result); // Verify the result is OkObjectResult

            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual("Document and associated file deleted successfully.", okResult.Value);

            // Verify the document is removed from the database
            var deletedDocument = await _dbContext.Documents.FindAsync(document.Id);
            Assert.IsNull(deletedDocument, "Document should be deleted from the database.");
        }


        [Test]
        public async Task DeleteDocument_Returns_NotFound_WhenDocumentDoesNotExist()
        {
            // Act
            var result = await _controller.DeleteDocument(9999);

            // Assert
            Assert.IsInstanceOf<NotFoundObjectResult>(result);
        }

        [Test]
        public async Task DeleteDocument_Returns_NotFound_WhenIdIsInvalid()
        {
            // Act: Call DeleteDocument with an invalid ID (like 9999)
            var result = await _controller.DeleteDocument(9999);

            // Assert: Expect a NotFoundObjectResult (404) instead of BadRequest (400)
            Assert.IsInstanceOf<NotFoundObjectResult>(result);
        }


        [Test]
        public async Task GetDocuments_Returns_ListOfDocuments()
        {
            // Arrange
            var document1 = new Document
            {
                Id = 1,
                FileName = "test1.pdf",
                FileContent = new byte[] { 0x1, 0x2, 0x3 }, // Required
                FilePath = "SharedDirectory/test1.pdf", // Required
                FileType = "pdf", // Required
                ContentType = "application/pdf", // Required
                UploadDate = DateTime.UtcNow,
                FileSize = 1024 // Optional but useful for testing
            };

            var document2 = new Document
            {
                Id = 2,
                FileName = "test2.pdf",
                FileContent = new byte[] { 0x4, 0x5, 0x6 }, // Required
                FilePath = "SharedDirectory/test2.pdf", // Required
                FileType = "pdf", // Required
                ContentType = "application/pdf", // Required
                UploadDate = DateTime.UtcNow,
                FileSize = 2048 // Optional but useful for testing
            };

            await _dbContext.Documents.AddRangeAsync(document1, document2);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _controller.GetDocuments();

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);

            var documents = okResult.Value as List<Document>;
            Assert.IsNotNull(documents);
            Assert.AreEqual(2, documents.Count);
            Assert.AreEqual("test1.pdf", documents[0].FileName);
            Assert.AreEqual("test2.pdf", documents[1].FileName);
        }


        [Test]
        public async Task GetDocuments_Returns_EmptyList_WhenNoDocumentsExist()
        {
            // Act
            var result = await _controller.GetDocuments();

            // Assert
            var okResult = result as OkObjectResult;
            var documents = okResult.Value as System.Collections.Generic.List<Document>;
            Assert.IsEmpty(documents);
        }

        [Test]
        public async Task Upload_SavesFileCorrectlyInDatabase()
        {
            // Arrange
            var fileMock = new Mock<Microsoft.AspNetCore.Http.IFormFile>();
            var content = "Sample file content";
            var fileName = "test.pdf";
            var memoryStream = new System.IO.MemoryStream();
            var writer = new System.IO.StreamWriter(memoryStream);
            writer.Write(content);
            writer.Flush();
            memoryStream.Position = 0;

            fileMock.Setup(f => f.OpenReadStream()).Returns(memoryStream);
            fileMock.Setup(f => f.FileName).Returns(fileName);
            fileMock.Setup(f => f.Length).Returns(memoryStream.Length);
            fileMock.Setup(f => f.ContentType).Returns("application/pdf");

            // Act
            await _controller.Upload(fileMock.Object);

            // Assert
            var savedDocument = await _dbContext.Documents.FirstOrDefaultAsync(d => d.FileName == fileName);
            Assert.IsNotNull(savedDocument);
            Assert.AreEqual(fileName, savedDocument.FileName);
        }

        [Test]
        public async Task DeleteDocument_DoesNotDeleteNonExistentFile()
        {
            // Act
            var result = await _controller.DeleteDocument(12345); // Non-existent document ID

            // Assert
            Assert.IsInstanceOf<NotFoundObjectResult>(result);
        }
    }
}
