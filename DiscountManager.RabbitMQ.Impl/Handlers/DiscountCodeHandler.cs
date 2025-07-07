using DiscountManager.Core;
using DiscountManager.Core.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DiscountManager.RabbitMQ.Impl.Handlers
{
    internal class DiscountCodeHandler : IRequestHandler<DiscountCodeRequest>
    {
        private readonly ILogger<DiscountCodeHandler> logger;
        private readonly DiscountDbContext dbContext;

        public DiscountCodeHandler(ILogger<DiscountCodeHandler> logger, DiscountDbContext dbContext)
        {
            ArgumentNullException.ThrowIfNull(logger, nameof(logger));
            ArgumentNullException.ThrowIfNull(dbContext, nameof(dbContext));

            this.logger = logger;
            this.dbContext = dbContext;
        }

        public async Task Handle(DiscountCodeRequest request, CancellationToken cancellationToken)
        {
            if (request.Codes == null || !request.Codes.Any())
            {
                if (logger.IsEnabled(LogLevel.Warning))
                {
                    logger.LogWarning("No code was found, skipping processing.");
                }

                return;
            }

            var discountCodes = request
                .Codes
                .Select(code => new DiscountCode
                {
                    Code = code,
                    State = Core.Enums.State.Active
                });

            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Received total codes: {0}", request.Codes.Count());
            }

            try
            {
                await dbContext.DiscountCodes.AddRangeAsync(discountCodes, cancellationToken);

                await dbContext.SaveChangesAsync(cancellationToken);

                if (logger.IsEnabled(LogLevel.Information))
                {
                    logger.LogInformation("All codes were persisted");
                }
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "An error happens while persisting the code.");
            }
        }
    }
}
