using System.Runtime.Serialization;

namespace DiscountManager.Core.Exceptions
{
    public class DiscountCodeSizeException : Exception
    {
        private const string message = "Discount code size must be between 7 and 8 characters. Current size is '{0}'";

        public DiscountCodeSizeException()
        {
        }

        public DiscountCodeSizeException(short size) : base(string.Format(message, size))
        {
        }

        public DiscountCodeSizeException(short size, Exception? innerException) : base(string.Format(message, size), innerException)
        {
        }
    }
}
