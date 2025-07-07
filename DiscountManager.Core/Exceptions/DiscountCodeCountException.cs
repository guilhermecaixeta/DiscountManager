using System.Runtime.Serialization;

namespace DiscountManager.Core.Exceptions
{
    public class DiscountCodeCountException : Exception
    {
        private const string message = "Discount code count must be between 1 and 2000. Current count is '{0}'";
        public DiscountCodeCountException()
        {
        }

        public DiscountCodeCountException(int count) : base(string.Format(message, count))
        {
        }

        public DiscountCodeCountException(int count, Exception? innerException) : base(string.Format(message, count), innerException)
        {
        }
    }
}
