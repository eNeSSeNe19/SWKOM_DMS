//using Moq;
//using NUnit.Framework;
//using SWKOM_DMS.Controllers;
//using SWKOM_DMS.Entities;
//using SWKOM_DMS.DTOs;
//using AutoMapper;
//using Microsoft.AspNetCore.Mvc;
//using System.Collections.Generic;
//using System.Threading.Tasks;
//using SWKOM_DMS.Services;
//using Microsoft.Extensions.Configuration;
//using System;
//using SWKOM_DMS.logging;

//namespace SWKOM_DMS.Tests
//{
//    [TestFixture]
//    public class DocumentControllerTests
//    {
//        private Mock<IDocumentRepository> _mockRepo;
//        private Mock<IMapper> _mockMapper;
//        private Mock<ILoggerWrapper> _mockLogger;  // Updated to use ILoggerWrapper
//        private Mock<ILoggerWrapper> _mockRabbitMqLogger;  // Updated to use ILoggerWrapper
//        private DocumentsController _controller;
//        private RabbitMQService _rabbitMqService;

//        [SetUp]
//        public void SetUp()
//        {
//            _mockRepo = new Mock<IDocumentRepository>();
//            _mockMapper = new Mock<IMapper>();
//            _mockLogger = new Mock<ILoggerWrapper>();  
//            _mockRabbitMqLogger = new Mock<ILoggerWrapper>();  

//            // Set up a mock configuration for RabbitMQService
//            var inMemorySettings = new Dictionary<string, string>
//            {
//                {"RabbitMQ:HostName", "localhost"},
//                {"RabbitMQ:QueueName", "documents_queue"}
//            };
//            IConfiguration configuration = new ConfigurationBuilder()
//                .AddInMemoryCollection(inMemorySettings)
//                .Build();

//            // Instantiate RabbitMQService with mock configuration and logger
//            _rabbitMqService = new RabbitMQService(configuration, _mockRabbitMqLogger.Object);

//            _controller = new DocumentsController(
//                _mockMapper.Object,
//                _mockLogger.Object,
//                _mockRepo.Object,
//                _rabbitMqService
//            );
//        }

//        [Test]
//        public async Task UploadDocument_Returns_OkResult_When_Valid()
//        {
//            // Arrange
//            var documentDto = new DocumentDto
//            {
//                FileName = "test.pdf",
//                FileType = "pdf",
//                ContentType = "application/pdf",
//                FileContent = new byte[] { 0x1, 0x2, 0x3 }
//            };

//            _mockMapper.Setup(m => m.Map<Document>(It.IsAny<DocumentDto>())).Returns(new Document());

//            // Act
//            var result = await _controller.UploadDocument(documentDto);

//            // Assert
//            Assert.IsInstanceOf<OkObjectResult>(result);
//        }

//        [Test]
//        public async Task UploadDocument_Returns_BadRequest_When_DocumentDto_Is_Null()
//        {
//            // Act
//            var result = await _controller.UploadDocument(null);

//            // Assert
//            Assert.IsInstanceOf<BadRequestObjectResult>(result);
//        }

//        [Test]
//        public async Task TestDbConnection_ShouldCallAddDocumentAsync()
//        {
//            // Arrange
//            var testDocument = new Document
//            {
//                FileName = "Test Document",
//                FileType = "pdf",
//                FileSize = 1000,
//                ContentType = "application/pdf",
//                FileContent = new byte[] { 0x1, 0x2, 0x3 },
//                UploadDate = DateTime.UtcNow
//            };

//            // Act
//            var result = await _controller.TestDbConnection();

//            // Assert
//            _mockRepo.Verify(r => r.AddDocumentAsync(It.Is<Document>(d =>
//                d.FileName == testDocument.FileName &&
//                d.FileType == testDocument.FileType &&
//                d.ContentType == testDocument.ContentType &&
//                d.FileSize == testDocument.FileSize
//            )), Times.Once);

//            Assert.IsInstanceOf<OkObjectResult>(result);
//        }

//        [Test]
//        public async Task UploadDocument_LogsInfoAndWarnMessages()
//        {
//            // Arrange
//            var documentDto = new DocumentDto
//            {
//                FileName = "test.pdf",
//                FileType = "pdf",
//                ContentType = "application/pdf",
//                FileContent = new byte[] { 0x1, 0x2, 0x3 }
//            };

//            _mockMapper.Setup(m => m.Map<Document>(It.IsAny<DocumentDto>())).Returns(new Document());

//            // Act
//            await _controller.UploadDocument(documentDto);

//            // Assert
//            _mockLogger.Verify(logger => logger.Info(It.Is<string>(s => s.Contains("Document saved to the database"))), Times.Once);
//            _mockLogger.Verify(logger => logger.Info(It.Is<string>(s => s.Contains("Message sent to RabbitMQ"))), Times.Once);
//        }

//        [Test]
//        public async Task UploadDocument_LogsWarn_WhenDocumentDtoIsNull()
//        {
//            // Act
//            await _controller.UploadDocument(null);

//            // Assert
//            _mockLogger.Verify(logger => logger.Warn(It.Is<string>(s => s.Contains("Upload attempted with a null DocumentDto."))), Times.Once);
//        }
//    }
//}
