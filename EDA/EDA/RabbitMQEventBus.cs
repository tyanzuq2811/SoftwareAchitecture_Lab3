using System.Text;
using System.Text.Json;
using EDA.Events;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace EDA
{
    public class RabbitMQEventBus : IEventBus, IDisposable
    {
        private readonly string _exchangeName = "eda.events";
        private IConnection? _connection;
        private IModel? _channel;
        private readonly EventBus _inMemoryFallback = new();
        private readonly bool _isConnected;

        public RabbitMQEventBus(string hostName = "localhost", int port = 5672, string userName = "guest", string password = "guest")
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = hostName,
                    Port = port,
                    UserName = userName,
                    Password = password,
                    RequestedConnectionTimeout = TimeSpan.FromSeconds(2)
                };

                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();
                _channel.ExchangeDeclare(exchange: _exchangeName, type: ExchangeType.Topic, durable: false);
                _isConnected = true;
                Console.WriteLine("[RabbitMQ] Kết nối thành công tới máy chủ RabbitMQ Broker!");
            }
            catch (Exception)
            {
                _isConnected = false;
                Console.WriteLine("[RabbitMQ] Không thể kết nối tới RabbitMQ Broker. Chuyển sang chế độ dự phòng EventBus In-Memory.");
            }
        }

        public void Publish<T>(T @event) where T : IEvent
        {
            if (!_isConnected || _channel == null)
            {
                _inMemoryFallback.Publish(@event);
                return;
            }

            var routingKey = typeof(T).Name;
            var message = JsonSerializer.Serialize(@event);
            var body = Encoding.UTF8.GetBytes(message);

            _channel.BasicPublish(
                exchange: _exchangeName,
                routingKey: routingKey,
                basicProperties: null,
                body: body);
        }

        public void Subscribe<T>(Action<T> handler) where T : IEvent
        {
            if (!_isConnected || _channel == null)
            {
                _inMemoryFallback.Subscribe(handler);
                return;
            }

            var routingKey = typeof(T).Name;
            var queueName = _channel.QueueDeclare(queue: "", durable: false, exclusive: true, autoDelete: true).QueueName;
            _channel.QueueBind(queue: queueName, exchange: _exchangeName, routingKey: routingKey);

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var @event = JsonSerializer.Deserialize<T>(message);
                if (@event != null)
                {
                    handler(@event);
                }
            };

            _channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);
        }

        public void Dispose()
        {
            _channel?.Close();
            _channel?.Dispose();
            _connection?.Close();
            _connection?.Dispose();
        }
    }
}
