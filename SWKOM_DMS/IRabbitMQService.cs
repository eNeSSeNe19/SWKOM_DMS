using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace SWKOM_DMS
{
    public interface IRabbitMQService
    {
        void SendMessage(string message);
        void ConsumeOcrResults();
    }
}
