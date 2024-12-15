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
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual("File uploaded successfully, and message sent to RabbitMQ.", okResult.Value);

            // Verify RabbitMQ Message Sent
            _mockRabbitMqService.Verify(r => r.SendMessage(It.IsAny<string>()), Times.Once);

            // Verify Document Saved
            var savedDocument = await _dbContext.Documents.FirstOrDefaultAsync(d => d.FileName == fileName);
            Assert.IsNotNull(savedDocument);
            Assert.AreEqual(fileName, savedDocument.FileName);
        }

        [Test]
        public async Task DeleteDocument_Returns_OkResult_And_RemovesDocumentFromDatabase()
        {
            // Arrange: Seed a document into the database with required properties
            var document = new Document
            {
                Id = 1,
                FileName = "test.pdf",
                FilePath = "SharedDirectory/test.pdf",
                ContentType = "application/pdf", // Add ContentType
                FileContent = new byte[] { 0x01, 0x02 }, // Add mock binary content
                FileType = "pdf" // Add FileType
            };

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

    }
}
