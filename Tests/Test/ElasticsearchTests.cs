using Elastic.Clients.Elasticsearch;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SWKOM_DMS.Tests
{
    [TestFixture]
    public class ElasticsearchTests
    {
        private ElasticsearchClient _elasticClient;

        [SetUp]
        public void SetUp()
        {
            // Elasticsearch-Client mit der URI des lokalen Elasticsearch-Servers
            var uri = "http://localhost:9200"; // oder der passende URI fÃ¼r deine Umgebung
            var settings = new ElasticsearchClientSettings(new Uri(uri));
            _elasticClient = new ElasticsearchClient(settings);
        }

        [Test]
        public async Task IndexDocument_ShouldIndexSuccessfully()
        {
            // Arrange
            var documentId = "123";
            var documentContent = new { ocrContent = "Sample OCR result content" };

            // Act
            var indexResponse = await _elasticClient.IndexAsync(documentContent, idx => idx.Index("ocr_results").Id(documentId));

            // Assert
            Assert.IsTrue(indexResponse.IsValidResponse, "Document indexing failed.");
        }

        [Test]
        public async Task SearchDocuments_ShouldReturnResults()
        {
            // Arrange
            var query = "Sample";
            var searchResults = new List<object>
            {
                new { ocrContent = "Sample OCR result content" }
            };

            // Act
            var searchResponse = await _elasticClient.SearchAsync<object>(s => s
                .Index("ocr_results")
                .Query(q => q
                    .Match(m => m
                        .Field("ocrContent")
                        .Query(query)
                    )
                )
            );

            // Assert
            Assert.IsTrue(searchResponse.IsValidResponse, "Search failed.");
            Assert.IsNotEmpty(searchResponse.Documents, "No documents were found.");
            Assert.AreEqual(1, searchResponse.Documents.Count(), "Unexpected number of documents.");
        }
    }
}