using MediatR;

namespace DiscountManager.RabbitMQ.Impl.Handlers
{
    public class DiscountCodeRequest : IRequest
    {
        public required IEnumerable<string> Codes { get; set; }
    }
}
