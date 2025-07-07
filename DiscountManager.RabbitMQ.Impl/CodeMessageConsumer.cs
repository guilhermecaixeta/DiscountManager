using DiscountManager.Core.Services;
using DiscountManager.RabbitMQ.Impl.Handlers;
using MediatR;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text.Json;

namespace DiscountManager.RabbitMQ.Impl
{
    internal class CodeMessageConsumer(IMediator mediator, ConnectionFactory connectionFactory, ILogger<CodeMessageConsumer> logger)
        : BaseMessageClient<CodeMessageConsumer>(connectionFactory, logger), IMessageConsumer
    {

        public async Task ConsumeAsync(CancellationToken token = default)
        {
            try
            {
                await Connect(token);

                var consumer = new AsyncEventingBasicConsumer(Channel);

                consumer.ReceivedAsync += OnReceivedEvent;

                await Channel.BasicConsumeAsync(
                    queue: Extensions.CodeQueue,
                    autoAck: false,
                    consumer: consumer,
                    cancellationToken: token);
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "Error while consuming message");
            }
        }

        public async Task OnReceivedEvent(object sender, BasicDeliverEventArgs @event)
        {
            try
            {
                var messages = JsonSerializer.Deserialize<IEnumerable<string>>(@event.Body.ToArray());

                var request = new DiscountCodeRequest
                {
                    Codes = messages ?? []
                };

                await mediator.Send(request, @event.CancellationToken);

                await Channel.BasicAckAsync(@event.DeliveryTag, false, @event.CancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "Error while retrieving message from queue.");

                await Channel.BasicNackAsync(@event.DeliveryTag, false, true, @event.CancellationToken);
            }
        }
    }
}
