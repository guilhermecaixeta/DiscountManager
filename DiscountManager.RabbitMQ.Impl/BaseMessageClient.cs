using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace DiscountManager.RabbitMQ.Impl
{
    public abstract class BaseMessageClient<TLogger>(
        ConnectionFactory connectionFactory,
        ILogger<TLogger> logger) : IDisposable
    {
        private IConnection connection;

        private readonly ConnectionFactory connectionFactory = connectionFactory;

        protected readonly ILogger<TLogger> logger = logger;

        protected IChannel Channel { get; private set; }

        public async Task Connect(CancellationToken token)
        {
            if (connection == null || connection.IsOpen == false)
            {
                connection = await connectionFactory.CreateConnectionAsync(token);
            }

            if (Channel == null)
            {
                Channel = await connection.CreateChannelAsync(cancellationToken: token);
                await Channel.ExchangeDeclareAsync(
                    exchange: Extensions.CodeCreationExchange,
                    type: ExchangeType.Direct,
                    durable: true,
                    autoDelete: false,
                    cancellationToken: token);

                await Channel.QueueDeclareAsync(
                    queue: Extensions.CodeQueue,
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    cancellationToken: token);

                await Channel.QueueBindAsync(
                    queue: Extensions.CodeQueue,
                    exchange: Extensions.CodeCreationExchange,
                    routingKey: Extensions.CodeQueueAndExchangeRoutingKey,
                    cancellationToken: token);
            }
        }

        public void Dispose()
        {
            try
            {
                Channel?.Dispose();
                Channel = null;

                connection?.Dispose();
                connection = null;
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "Cannot dispose RabbitMQ channel or connection");
            }
        }
    }
}
