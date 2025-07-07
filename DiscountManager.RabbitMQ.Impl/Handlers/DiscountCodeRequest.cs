using MediatR;

namespace DiscountManager.RabbitMQ.Impl.Handlers
{
    public class DiscountCodeRequest : IRequest
    {
        public IEnumerable<string> Codes { get; set; }
    }
}
