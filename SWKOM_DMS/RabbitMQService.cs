using RabbitMQ.Client;
using System.Text;
using Microsoft.Extensions.Configuration;
using System;
using SWKOM_DMS.logging; 

namespace SWKOM_DMS.Services
{
    public class RabbitMQService
    {
        private readonly string _hostname;
        private readonly string _queueName;
        private IConnection _connection;
        private IModel _channel;
        private readonly ILoggerWrapper _logger;

        public RabbitMQService(IConfiguration configuration, ILoggerWrapper logger)
        {
            _hostname = configuration["RabbitMQ:HostName"];
            _queueName = configuration["RabbitMQ:QueueName"];
            _logger = logger;
            CreateConnection();
        }

        private void CreateConnection()
        {
            try
            {
                var factory = new ConnectionFactory() { HostName = _hostname };
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();
                _channel.QueueDeclare(queue: _queueName,
                                      durable: false,
                                      exclusive: false,
                                      autoDelete: false,
                                      arguments: null);
                _logger.Info("RabbitMQ connection and queue declaration successful.");
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to connect to RabbitMQ.", ex);
                throw new ApplicationException("Could not establish a connection to RabbitMQ.", ex);
            }
        }

        public void SendMessage(string message)
        {
            if (_channel == null)
            {
                _logger.Error("RabbitMQ channel is not available. Message not sent.", null);
                throw new InvalidOperationException("RabbitMQ channel is not available.");
            }

            try
            {
                var body = Encoding.UTF8.GetBytes(message);
                _channel.BasicPublish(exchange: "",
                                      routingKey: _queueName,
                                      basicProperties: null,
                                      body: body);
                _logger.Info($"Message sent to RabbitMQ queue: {_queueName}");
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to send message to RabbitMQ.", ex);
                throw;
            }
        }

        public void CloseConnection()
        {
            try
            {
                _channel?.Close();
                _connection?.Close();
                _logger.Info("RabbitMQ connection closed successfully.");
            }
            catch (Exception ex)
            {
                _logger.Error("Error occurred while closing RabbitMQ connection.", ex);
            }
        }
    }
}

