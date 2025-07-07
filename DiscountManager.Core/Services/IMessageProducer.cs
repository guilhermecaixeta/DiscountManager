namespace DiscountManager.Core.Services
{
    public interface IMessageProducer<T>
        where T : class
    {
        Task PublishAsync(T message, CancellationToken token = default);
    }
}
