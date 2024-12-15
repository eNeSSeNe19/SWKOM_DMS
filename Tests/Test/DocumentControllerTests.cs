using Moq;
using NUnit.Framework;
using SWKOM_DMS.Controllers;
using SWKOM_DMS.Entities;
using SWKOM_DMS.Services;
using SWKOM_DMS.logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Elastic.Clients.Elasticsearch;
using Microsoft.AspNetCore.Http;
using Elasticsearch.Net;
using Microsoft.EntityFrameworkCore.InMemory;


namespace SWKOM_DMS.Tests
{

    [TestFixture]
    public class DocumentsControllerTests
    {
        private DocumentsController _controller;
        private Mock<IDocumentRepository> _mockRepo;
        private Mock<ILoggerWrapper> _mockLogger;
        //private Mock<RabbitMQService> _mockRabbitMqService;
        //private Mock<ElasticsearchClientProvider> _mockElasticProvider;
        private Mock<ElasticsearchClient> _mockElasticClient;

        private Mock<IElasticsearchClientProvider> _mockElasticProvider;

        private Mock<DocumentDbContext> _mockDbContext;

        private DocumentDbContext _dbContext;


        private Mock<IRabbitMQService> _mockRabbitMqService;


        [TearDown]
        public void TearDown()
        {
            _dbContext?.Dispose();
        }



        [SetUp]
public void SetUp()
{

    _mockRepo = new Mock<IDocumentRepository>();
    _mockLogger = new Mock<ILoggerWrapper>();
    _mockRabbitMqService = new Mock<IRabbitMQService>();
    _mockElasticProvider = new Mock<IElasticsearchClientProvider>();
    _mockElasticClient = new Mock<Elastic.Clients.Elasticsearch.ElasticsearchClient>();

    // Use an actual in-memory database context
    var options = new DbContextOptionsBuilder<DocumentDbContext>()
        .UseInMemoryDatabase("TestDb")
        .Options;

    _dbContext = new DocumentDbContext(options); // Real in-memory context

    _mockElasticProvider.Setup(e => e.GetClient()).Returns(_mockElasticClient.Object);

    // Instantiate the controller
    _controller = new DocumentsController(
        null, // Assuming AutoMapper isn't used here
        _mockLogger.Object,
        _mockRepo.Object,
        _mockRabbitMqService.Object,
        _dbContext,
        _mockElasticProvider.Object
    );
}



        [Test]
        public async Task GetDocuments_Returns_OkResult_WithDocumentList()
        {
            // Arrange
            var documents = new List<Document>
            {
                new Document { Id = 1, FileName = "test1.pdf" },
                new Document { Id = 2, FileName = "test2.pdf" }
            };

            _mockRepo.Setup(repo => repo.GetAllDocumentsAsync())
                .ReturnsAsync(documents);

            // Act
            var result = await _controller.GetDocuments();

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var returnedDocs = okResult.Value as List<Document>;
            Assert.AreEqual(2, returnedDocs.Count);
        }

        [Test]
        public async Task Upload_Returns_OkResult_WhenFileIsUploaded()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            var content = "Fake file content";
            var fileName = "test.pdf";
            var memoryStream = new MemoryStream();
            var writer = new StreamWriter(memoryStream);
            writer.Write(content);
            writer.Flush();
            memoryStream.Position = 0;

            fileMock.Setup(f => f.OpenReadStream()).Returns(memoryStream);
            fileMock.Setup(f => f.FileName).Returns(fileName);
            fileMock.Setup(f => f.Length).Returns(memoryStream.Length);
            fileMock.Setup(f => f.ContentType).Returns("application/pdf");

            // Use InMemoryDatabase instead of mocking DbContext
            var options = new DbContextOptionsBuilder<DocumentDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;

            using var dbContext = new DocumentDbContext(options);

            // Reinitialize the controller with the real in-memory DbContext
            _controller = new DocumentsController(
                null, // Assuming AutoMapper isn't used here
                _mockLogger.Object,
                _mockRepo.Object,
                _mockRabbitMqService.Object,
                dbContext,
                _mockElasticProvider.Object
            );

            _mockRabbitMqService.Setup(s => s.SendMessage(It.IsAny<string>())).Verifiable();

            // Act
            var result = await _controller.Upload(fileMock.Object);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<OkObjectResult>(result); // Ensure OkObjectResult is returned

            var okResult = result as OkObjectResult;
            Assert.AreEqual("File uploaded successfully, and message sent to RabbitMQ.", okResult.Value);

            _mockRabbitMqService.Verify(s => s.SendMessage(It.IsAny<string>()), Times.Once);
        }









        [Test]
        public async Task SearchDocuments_Returns_Results_From_Elasticsearch()
        {
            // Arrange
            string query = "test";
            var mockSearchResponse = new SearchResponse<object>();

            _mockElasticClient.Setup(e => e.SearchAsync<object>(
                It.IsAny<SearchRequest>(),
                default
            )).ReturnsAsync(mockSearchResponse);

            // Act
            var result = await _controller.SearchDocuments(query);

            // Assert
            Assert.IsInstanceOf<ObjectResult>(result);
        }


        [Test]
        public async Task DeleteDocument_Returns_OkResult_WhenDocumentIsDeleted()
        {
            // Arrange
            int documentId = 1;
            var document = new Document
            {
                Id = documentId,
                FileName = "test.pdf",
                FilePath = "SharedDirectory/test.pdf",
                FileType = "pdf", // Add required properties
                ContentType = "application/pdf",
                FileContent = new byte[] { 0x1, 0x2, 0x3 }, // Mock file content
                UploadDate = DateTime.UtcNow,
                FileSize = 1234
            };

            // Set up InMemoryDatabase for DocumentDbContext
            var options = new DbContextOptionsBuilder<DocumentDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_DeleteDocument")
                .Options;

            using var dbContext = new DocumentDbContext(options);
            dbContext.Documents.Add(document); // Add the document to the in-memory database
            await dbContext.SaveChangesAsync();

            // Reinitialize controller with the real in-memory dbContext
            _controller = new DocumentsController(
                null, // AutoMapper is not used here
                _mockLogger.Object,
                _mockRepo.Object,
                _mockRabbitMqService.Object,
                dbContext,
                _mockElasticProvider.Object
            );

            // Act
            var result = await _controller.DeleteDocument(documentId);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            _mockLogger.Verify(logger => logger.Info(It.Is<string>(s => s.Contains("deleted"))), Times.Once);

            // Verify that the document is removed from the database
            var deletedDocument = await dbContext.Documents.FindAsync(documentId);
            Assert.IsNull(deletedDocument); // The document should no longer exist
        }



        [Test]
        public async Task Upload_Returns_BadRequest_WhenFileIsNull()
        {
            // Act
            var result = await _controller.Upload(null);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }
    }
}
