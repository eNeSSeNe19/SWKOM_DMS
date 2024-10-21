using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NUnit.Framework;
using SWKOM_DMS.Entities;

namespace SWKOM_DMS.Tests
{
    public class DocumentValidationTests
    {
        private List<ValidationResult> ValidateModel(object model)
        {
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(model, null, null);
            Validator.TryValidateObject(model, validationContext, validationResults, true);
            return validationResults;
        }

        [Test]
        public void Should_Throw_Error_When_FileContent_Is_Null()
        {
            var document = new Document
            {
                FileName = "document.pdf",
                FileType = "pdf",
                FileSize = 1000,
                ContentType = "application/pdf",
                FileContent = null, // Invalid case
                UploadDate = DateTime.Now
            };

            var result = ValidateModel(document);

            Assert.That(result, Has.Some.Matches<ValidationResult>(v => v.ErrorMessage.Contains("FileContent is required")));
        }

        [Test]
        public void Should_Pass_With_Valid_Document()
        {
            var document = new Document
            {
                FileName = "document.pdf",
                FileType = "pdf",
                FileSize = 1000,
                ContentType = "application/pdf",
                FileContent = new byte[] { 0x1, 0x2, 0x3 }, // Valid byte array
                UploadDate = DateTime.Now
            };

            var result = ValidateModel(document);

            Assert.IsEmpty(result); // Should pass validation
        }
    }
}
