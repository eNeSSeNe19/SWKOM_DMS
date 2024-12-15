//using Elastic.Clients.Elasticsearch;
//using NUnit.Framework;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;

//namespace SWKOM_DMS.Tests
//{
//    [TestFixture]
//    public class ElasticsearchTests
//    {
//        private ElasticsearchClient _elasticClient;

//        [SetUp]
//        public void SetUp()
//        {
//            // Elasticsearch-Client mit der URI des lokalen Elasticsearch-Servers
//            var uri = "http://localhost:9200"; // oder der passende URI fÃ¼r deine Umgebung
//            var settings = new ElasticsearchClientSettings(new Uri(uri));
//            _elasticClient = new ElasticsearchClient(settings);
//        }

//        [Test]
//        public async Task IndexDocument_ShouldIndexSuccessfully()
//        {
//            // Arrange
//            var documentId = "123";
//            var documentContent = new { ocrContent = "Sample OCR result content" };

//            // Act
//            var indexResponse = await _elasticClient.IndexAsync(documentContent, idx => idx.Index("ocr_results").Id(documentId));

//            // Assert
//            Assert.IsTrue(indexResponse.IsValidResponse, "Document indexing failed.");
//        }

//        [Test]
//        public async Task SearchDocuments_ShouldReturnResults()
//        {
//            // Arrange
//            var query = "Sample";
//            var searchResults = new List<object>
//            {
//                new { ocrContent = "Sample OCR result content" }
//            };

//            // Act
//            var searchResponse = await _elasticClient.SearchAsync<object>(s => s
//                .Index("ocr_results")
//                .Query(q => q
//                    .Match(m => m
//                        .Field("ocrContent")
//                        .Query(query)
//                    )
//                )
//            );

//            // Assert
//            Assert.IsTrue(searchResponse.IsValidResponse, "Search failed.");
//            Assert.IsNotEmpty(searchResponse.Documents, "No documents were found.");
//            Assert.AreEqual(1, searchResponse.Documents.Count(), "Unexpected number of documents.");
//        }
//    }
//}

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
            var uri = "http://localhost:9200"; // Local Elasticsearch instance
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
        }


        [Test]
        public async Task SearchDocuments_ReturnsEmptyResults_WhenQueryDoesNotMatch()
        {
            // Arrange
            var query = "ThisQueryShouldNotMatchAnything";

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
            Assert.IsEmpty(searchResponse.Documents, "Expected no matching documents.");
        }

        [Test]
        public async Task SearchDocuments_ReturnsInvalidResponse_WhenIndexDoesNotExist()
        {
            // Act
            var response = await _elasticClient.SearchAsync<object>(s => s
                .Index("non_existent_index")
                .Query(q => q
                    .Match(m => m
                        .Field("ocrContent")
                        .Query("Some query")
                    )
                )
            );

            // Assert
            Assert.IsFalse(response.IsValidResponse, "Expected the search to return an invalid response for a non-existent index.");
            Assert.NotNull(response.ElasticsearchServerError, "Expected an error in the server response.");
            StringAssert.Contains("index_not_found_exception", response.ElasticsearchServerError?.Error?.Type);
        }


        [Test]
        public async Task IndexDocument_ReturnsError_WhenDocumentIsNull()
        {
            // Act
            var response = await _elasticClient.IndexAsync<object>(null, idx => idx.Index("ocr_results"));

            // Assert
            Assert.IsFalse(response.IsValidResponse, "Expected indexing a null document to return an invalid response.");
        }


        [Test]
        public async Task DeleteDocument_ShouldDeleteSuccessfully()
        {
            // Arrange
            var documentId = "567";
            var documentContent = new { ocrContent = "Content to delete" };
            await _elasticClient.IndexAsync(documentContent, idx => idx.Index("ocr_results").Id(documentId));

            // Act
            var deleteResponse = await _elasticClient.DeleteAsync<object>(documentId, d => d.Index("ocr_results"));

            // Assert
            Assert.IsTrue(deleteResponse.IsValidResponse, "Document deletion failed.");
        }

        [Test]
        public async Task DeleteDocument_ReturnsNotFound_WhenDocumentDoesNotExist()
        {
            // Arrange
            var documentId = "non_existent_document";

            // Act
            var deleteResponse = await _elasticClient.DeleteAsync<object>(documentId, d => d.Index("ocr_results"));

            // Assert
            Assert.IsFalse(deleteResponse.IsValidResponse, "Expected the deletion to fail for a non-existent document.");
        }

        [Test]
        public async Task BulkIndexDocuments_ShouldIndexMultipleSuccessfully()
        {
            // Arrange
            var documents = new List<object>
            {
                new { Id = "1", ocrContent = "Bulk OCR content 1" },
                new { Id = "2", ocrContent = "Bulk OCR content 2" },
                new { Id = "3", ocrContent = "Bulk OCR content 3" }
            };

            // Act
            var bulkResponse = await _elasticClient.BulkAsync(b => b
                .Index("ocr_results")
                .IndexMany(documents)
            );

            // Assert
            Assert.IsTrue(bulkResponse.IsValidResponse, "Bulk indexing failed.");
        }

        

        [Test]
        public async Task GetDocumentById_ReturnsDocument_WhenDocumentExists()
        {
            // Arrange
            var documentId = "555";
            var documentContent = new { ocrContent = "Sample document content" };
            await _elasticClient.IndexAsync(documentContent, idx => idx.Index("ocr_results").Id(documentId));

            // Act
            var getResponse = await _elasticClient.GetAsync<object>(documentId, g => g.Index("ocr_results"));

            // Assert
            Assert.IsTrue(getResponse.Found, "Document not found.");
            Assert.IsNotNull(getResponse.Source, "Document source is null.");
        }

        [Test]
        public async Task GetDocumentById_ReturnsNull_WhenDocumentDoesNotExist()
        {
            // Act
            var getResponse = await _elasticClient.GetAsync<object>("non_existent_id", g => g.Index("ocr_results"));

            // Assert
            Assert.IsFalse(getResponse.Found, "Expected no document to be found.");
            Assert.IsNull(getResponse.Source, "Expected the document source to be null.");
        }
    }
}
