using NUnit.Framework;
using Tesseract;
using System.IO;

namespace Tests
{
    [TestFixture]
    public class OcrWorkerTests
    {
        private string _testImagePath;

        [SetUp]
        public void Setup()
        {
            // Use the absolute path to the test image
            _testImagePath = @"C:\Users\eNeSSeNe\Desktop\SWKOM_DMS\SWKOM_DMS\SharedDirectory\images\test-image.png";

            // Check if the file exists before running the test
            Assert.IsTrue(File.Exists(_testImagePath), "Test image not found at: " + _testImagePath);
        }

        [Test]
        public void PerformOcr_ReturnsText_WhenImageIsValid()
        {
            // Arrange
            var tessDataPath = @"C:\Users\eNeSSeNe\Desktop\SWKOM_DMS\SWKOM_DMS\OCRWorker\tessdata\";
            var testImagePath = @"C:\Users\eNeSSeNe\Desktop\SWKOM_DMS\SWKOM_DMS\SharedDirectory\images\test-image.png";

            // Expected partial text to validate OCR functionality
            var expectedTextSnippet = "ROLLENSPIELE";

            Assert.IsTrue(File.Exists(testImagePath), "Test image not found.");
            Assert.IsTrue(Directory.Exists(tessDataPath), "Tessdata folder not found.");

            string actualText;

            // Act
            using (var engine = new TesseractEngine(tessDataPath, "eng", EngineMode.Default))
            {
                using (var img = Pix.LoadFromFile(testImagePath))
                {
                    using (var page = engine.Process(img))
                    {
                        actualText = page.GetText().Trim();
                        Console.WriteLine($"OCR Output: {actualText}");
                    }
                }
            }

            // Assert
            Assert.IsNotNull(actualText);
            StringAssert.Contains(expectedTextSnippet, actualText,
                "OCR did not detect the expected text in the image.");
        }

        [Test]
        public void PerformOcr_ThrowsException_WhenTessDataPathIsInvalid()
        {
            // Arrange
            var invalidTessDataPath = @"C:\InvalidPath\tessdata";
            var testImagePath = _testImagePath;

            // Act & Assert
            var ex = Assert.Throws<TesseractException>(() =>
            {
                using (var engine = new TesseractEngine(invalidTessDataPath, "eng", EngineMode.Default))
                using (var img = Pix.LoadFromFile(testImagePath))
                using (var page = engine.Process(img))
                {
                    _ = page.GetText();
                }
            });

            StringAssert.Contains("Failed to initialise tesseract engine", ex.Message);
        }

        [Test]
        public void PerformOcr_ThrowsException_WhenImageFileIsMissing()
        {
            // Arrange
            var tessDataPath = @"C:\Users\eNeSSeNe\Desktop\SWKOM_DMS\SWKOM_DMS\OCRWorker\tessdata\";
            var invalidImagePath = @"C:\InvalidPath\missing-image.png"; // Invalid path

            Assert.IsTrue(Directory.Exists(tessDataPath), "Tessdata folder not found.");

            // Act & Assert
            var exception = Assert.Throws<IOException>(() =>
            {
                using (var engine = new TesseractEngine(tessDataPath, "eng", EngineMode.Default))
                using (var img = Pix.LoadFromFile(invalidImagePath))
                using (var page = engine.Process(img))
                {
                    // This part should not execute as exception is expected
                }
            });

            // Additional Verification (optional)
            Assert.That(exception.Message, Does.Contain("Failed to load image"),
                "Exception message does not match the expected content.");
        }



        



        [Test]
        public void PerformOcr_ReturnsText_WhenImageContainsEnglishText()
        {
            // Arrange
            var tessDataPath = @"C:\Users\eNeSSeNe\Desktop\SWKOM_DMS\SWKOM_DMS\OCRWorker\tessdata\";
            var englishImagePath = _testImagePath; // Assuming test image contains English text
            var expectedTextSnippet = "ROLLENSPIELE"; // Still testing against "ROLLENSPIELE"

            // Validate that the required files exist
            Assert.IsTrue(File.Exists(englishImagePath), "Test image not found.");
            Assert.IsTrue(Directory.Exists(tessDataPath), "Tessdata folder not found.");

            string actualText;

            // Act
            using (var engine = new TesseractEngine(tessDataPath, "eng", EngineMode.Default))
            using (var img = Pix.LoadFromFile(englishImagePath))
            using (var page = engine.Process(img))
            {
                actualText = page.GetText().Trim();
                Console.WriteLine($"OCR Output: {actualText}");
            }

            // Assert
            Assert.IsNotNull(actualText, "OCR output is null.");
            StringAssert.Contains(expectedTextSnippet, actualText,
                "OCR did not detect the expected text in the image using the English language.");
        }





    }
}
