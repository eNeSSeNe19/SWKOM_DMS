using Elastic.Clients.Elasticsearch;

namespace SWKOM_DMS.Services
{
    public class ElasticsearchClientProvider : IElasticsearchClientProvider
    {
        private readonly ElasticsearchClient _client;

        public ElasticsearchClientProvider(string uri)
        {
            if (string.IsNullOrWhiteSpace(uri))
            {
                throw new ArgumentException("Elasticsearch URI cannot be null or empty.");
            }

            var settings = new ElasticsearchClientSettings(new Uri(uri));
            _client = new ElasticsearchClient(settings);
        }

        public ElasticsearchClient GetClient()
        {
            return _client;
        }
    }
}
