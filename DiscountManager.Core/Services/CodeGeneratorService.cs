using DiscountManager.Core.Exceptions;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace DiscountManager.Core.Services
{
    public class CodeGeneratorService : ICodeGeneratorService
    {
        private const uint MAX_GENERATE_COUNT = 2_000;
        private const uint MAX_LENGTH = 8;
        private const uint MIN_LENGTH = 7;

        private readonly byte[] secretKey;

        public CodeGeneratorService(IOptions<GeneratorOptions> options)
        {
            ArgumentNullException.ThrowIfNull(options, nameof(IOptions<GeneratorOptions>));
            ArgumentNullException.ThrowIfNull(options.Value, nameof(GeneratorOptions));
            ArgumentException.ThrowIfNullOrEmpty(options.Value.SecretKey, nameof(options.Value.SecretKey));

            secretKey = Encoding.ASCII.GetBytes(options.Value.SecretKey);
        }

        public string GenerateCode(short size)
        {
            if (size < MIN_LENGTH || size > MAX_LENGTH)
            {
                throw new DiscountCodeSizeException(size);
            }

            var randomBytes = Guid
                      .NewGuid()
                      .ToByteArray();

            var timestamp = DateTimeOffset
                .UtcNow
                .ToUnixTimeSeconds();

            var timestampBytes = BitConverter.GetBytes(timestamp);

            var byteArray = secretKey
                .Concat(randomBytes)
                .Concat(timestampBytes)
                .ToArray();

            var hashBytes = SHA1.HashData(byteArray);

            var code = BitConverter
                .ToString(hashBytes)
                .Replace("-", "")
                .ToUpper()
                .Take(size);

            return string.Join(string.Empty, code);
        }

        public IEnumerable<string> GenerateCodes(int count, short size)
        {
            if (count < 1 || count > MAX_GENERATE_COUNT)
            {
                throw new DiscountCodeCountException(count);
            }

            for (int i = 0; i < count; i++)
            {
                yield return GenerateCode(size);
            }
        }
    }
}
