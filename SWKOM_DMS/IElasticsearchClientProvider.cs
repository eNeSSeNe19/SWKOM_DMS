using Elastic.Clients.Elasticsearch;

namespace SWKOM_DMS
{
    public interface IElasticsearchClientProvider
    {
        ElasticsearchClient GetClient();
    }
}
