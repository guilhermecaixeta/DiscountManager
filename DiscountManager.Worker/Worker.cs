using DiscountManager.Core.Services;

namespace DiscountManager.Worker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> logger;
        private readonly IMessageConsumer messageConsumer;

        public Worker(IMessageConsumer messageConsumer, ILogger<Worker> logger)
        {
            ArgumentNullException.ThrowIfNull(logger, nameof(logger));
            ArgumentNullException.ThrowIfNull(messageConsumer, nameof(messageConsumer));

            this.logger = logger;
            this.messageConsumer = messageConsumer;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            }

            await messageConsumer.ConsumeAsync(stoppingToken);
        }
    }
}
