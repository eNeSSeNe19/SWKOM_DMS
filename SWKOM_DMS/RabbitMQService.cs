using RabbitMQ.Client;
using System.Text;

namespace SWKOM_DMS.Services
{
    public class RabbitMQService
    {
        private readonly string _hostname;
        private readonly string _queueName;
        private IConnection _connection;
        private IModel _channel;

        public RabbitMQService(IConfiguration configuration)
        {
            _hostname = configuration["RabbitMQ:HostName"];
            _queueName = configuration["RabbitMQ:QueueName"];
            CreateConnection();
        }

        private void CreateConnection()
        {
            var factory = new ConnectionFactory() { HostName = _hostname };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: _queueName,
                                  durable: false,
                                  exclusive: false,
                                  autoDelete: false,
                                  arguments: null);
        }

        public void SendMessage(string message)
        {
            var body = Encoding.UTF8.GetBytes(message);

            _channel.BasicPublish(exchange: "",
                                  routingKey: _queueName,
                                  basicProperties: null,
                                  body: body);
        }

        public void CloseConnection()
        {
            _channel.Close();
            _connection.Close();
        }
    }
}
