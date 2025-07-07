using DiscountManager.Core.Services;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace DiscountManager.RabbitMQ.Impl
{
    internal class CodeMessageProducer : BaseMessageClient<CodeMessageProducer>, IMessageProducer<IEnumerable<string>>
    {
        protected string ExchangeName => Extensions.CodeCreationExchange;
        protected string RoutingKeyName => Extensions.CodeQueueAndExchangeRoutingKey;
        protected string AppId => "DiscountManager.RabbitMQ.Impl.CodeMessageProducer";

        public CodeMessageProducer(ConnectionFactory connectionFactory, ILogger<CodeMessageProducer> logger)
            : base(connectionFactory, logger)
        {
        }

        public async Task PublishAsync(IEnumerable<string> message, CancellationToken token = default)
        {
            try
            {
                await Connect(token);

                var properties = new BasicProperties
                {
                    AppId = AppId,
                    ContentType = "application/json",
                    DeliveryMode = DeliveryModes.Transient,
                    Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                };

                var count = 1;

                var chunkSize = message.Count() / 100;

                var chunk = message.Chunk(chunkSize > 1 ? chunkSize : 20).ToList();

                foreach (var item in chunk)
                {
                    logger.LogInformation($"Publishing chunk: '{count}/{chunk.Count}'");

                    var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(item));

                    await Channel.BasicPublishAsync(
                        exchange: ExchangeName,
                        routingKey: RoutingKeyName,
                        mandatory: true,
                        body: body,
                        basicProperties: properties,
                        cancellationToken: token);

                    count++;
                }

                logger.LogInformation($"Publishing succesfully");
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "Error while publishing");
            }
        }
    }
}
