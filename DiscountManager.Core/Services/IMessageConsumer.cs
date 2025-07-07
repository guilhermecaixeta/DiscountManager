namespace DiscountManager.Core.Services
{
    public interface IMessageConsumer
    {
        Task ConsumeAsync(CancellationToken token = default);
    }
}
