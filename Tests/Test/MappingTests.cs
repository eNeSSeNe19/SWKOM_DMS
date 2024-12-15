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
                FileType = "pdf",
                ContentType = "application/pdf",
                // FileContent als Base64-String, nicht als byte[] übergeben
                FileContent = Convert.ToBase64String(new byte[] { 0x01, 0x02, 0x03 })
            };

            // Act
            var documentEntity = _mapper.Map<Document>(documentDto);

            // Assert
            Assert.AreEqual(documentDto.FileName, documentEntity.FileName);
            Assert.AreEqual(documentDto.ContentType, documentEntity.ContentType);

            // Vergleich der byte[] Arrays
            var expectedFileContent = Convert.FromBase64String(documentDto.FileContent);  // Base64 string zu byte[]
            Assert.AreEqual(expectedFileContent.Length, documentEntity.FileContent.Length);
            for (int i = 0; i < expectedFileContent.Length; i++)
            {
                Assert.AreEqual(expectedFileContent[i], documentEntity.FileContent[i]);
            }
        }

    }
}