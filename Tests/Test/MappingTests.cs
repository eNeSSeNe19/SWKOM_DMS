using AutoMapper;
using NUnit.Framework;
using SWKOM_DMS;
using SWKOM_DMS.DTOs;
using SWKOM_DMS.Entities;

namespace SWKOM_DMS.Tests
{
    [TestFixture]
    public class MappingTests
    {
        private IMapper _mapper;

        [SetUp]
        public void SetUp()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
            _mapper = config.CreateMapper();
        }

        [Test]
        public void DocumentDto_To_Document_Mapping_IsValid()
        {
            // Arrange
            var documentDto = new DocumentDto
            {
                FileName = "test.pdf",
                ContentType = "application/pdf",
                FileContent = new byte[] { 0x01, 0x02, 0x03 }
            };

            // Act
            var documentEntity = _mapper.Map<Document>(documentDto);

            // Assert
            Assert.AreEqual(documentDto.FileName, documentEntity.FileName);
            Assert.AreEqual(documentDto.ContentType, documentEntity.ContentType);
            Assert.AreEqual(documentDto.FileContent, documentEntity.FileContent);
        }
    }
}
