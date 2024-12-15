using Elastic.Clients.Elasticsearch;
using System.Threading.Tasks;

namespace SWKOM_DMS
{
    public class IndexingWorker
    {
        private readonly ElasticsearchClient _elasticClient;

        public IndexingWorker(IElasticsearchClientProvider elasticClientProvider)
        {
            _elasticClient = elasticClientProvider.GetClient();
        }

        public async Task IndexOcrResult(string documentId, string ocrContent)
        {
            var document = new
            {
                Id = documentId,
                OcrContent = ocrContent
            };

            // Use IndexRequest with the correct type argument
            var indexRequest = new IndexRequest<object>("ocr_results") { Document = document };

            var response = await _elasticClient.IndexAsync(indexRequest);

            if (!response.IsValidResponse)
            {
                throw new System.Exception($"Failed to index document: {response.ElasticsearchServerError?.Error.Reason}");
            }
        }
    }
}
