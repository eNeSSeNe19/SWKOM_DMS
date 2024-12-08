using Elastic.Clients.Elasticsearch;

public class ElasticsearchClientProvider
{
    private readonly ElasticsearchClient _client;

    public ElasticsearchClientProvider(string uri)
    {
        if (string.IsNullOrWhiteSpace(uri))
        {
            throw new ArgumentException("Elasticsearch URI cannot be null or empty.");
        }

        try
        {
            var settings = new ElasticsearchClientSettings(new Uri(uri));
            _client = new ElasticsearchClient(settings);
            Console.WriteLine($"Successfully initialized Elasticsearch client with URI: {uri}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to initialize Elasticsearch client. Error: {ex.Message}");
            throw;
        }
    }

    public ElasticsearchClient GetClient() => _client;
}
