namespace DiscountManager.Core.Services
{
    public interface ICodeGeneratorService
    {
        /// <summary>
        /// Generates a unique discount code.
        /// </summary>
        /// <returns>A unique discount code as a string.</returns>
        string GenerateCode(short size);

        /// <summary>
        /// Generates a specified number of unique discount codes of a given size.
        /// </summary>
        /// <param name="count">Total number of code to be generated.</param>
        /// <param name="size">The code size, must be between 7 and 8 characters.</param>
        /// <returns>A list of code given the size and count</returns>
        IEnumerable<string> GenerateCodes(int count, short size);
    }
}
