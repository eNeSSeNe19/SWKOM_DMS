using System;
using NUnit.Framework;
using SWKOM_DMS.Entities;

namespace SWKOM_DMS.Tests
{
    [TestFixture]
    public class DocumentTests
    {
        [Test]
        public void Document_CanBeCreatedAndHasCorrectProperties()
        {
            // Arrange
            var fileName = "test.pdf";
            var fileContent = new byte[] { 0x01, 0x02, 0x03 };
            var uploadDate = DateTime.UtcNow;
            var contentType = "application/pdf";

            // Act
            var document = new Document
            {
                FileName = fileName,
                FileContent = fileContent,
                UploadDate = uploadDate,
                ContentType = contentType
            };

            // Assert
            Assert.AreEqual(fileName, document.FileName);
            Assert.AreEqual(fileContent, document.FileContent);
            Assert.AreEqual(uploadDate, document.UploadDate);
            Assert.AreEqual(contentType, document.ContentType);
        }
    }
}
