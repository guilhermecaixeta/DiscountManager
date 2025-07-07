using DiscountManager.Core.Exceptions;
using DiscountManager.Core.Services;
using Microsoft.Extensions.Options;
using Moq;

namespace DiscountManager.Core.UnitTests.Services
{
    public class CodeGeneratorServiceTests
    {
        [Theory]
        [InlineData(7)]
        [InlineData(8)]
        public void GenerateCode_ShouldReturnValidCode(short size)
        {
            // Arrange
            var optionsMock = new Mock<IOptions<GeneratorOptions>>();

            var fakeGeneratorOptions = new GeneratorOptions { SecretKey = "test" };

            optionsMock.Setup(f => f.Value)
                       .Returns(fakeGeneratorOptions);

            var service = new CodeGeneratorService(optionsMock.Object);

            // Act
            var code = service.GenerateCode(size);

            // Assert
            Assert.NotNull(code);
            Assert.Matches(@$"^[A-Z0-9]{{{size}}}$", code);
        }

        [Theory]
        [InlineData(6)]
        [InlineData(9)]
        public void GenerateCode_ShouldThrowDiscountCodeSizeException_WhenSizeIsInvalid(short size)
        {
            // Arrange
            var optionsMock = new Mock<IOptions<GeneratorOptions>>();
            var fakeGeneratorOptions = new GeneratorOptions { SecretKey = "test" };

            optionsMock.Setup(f => f.Value)
                       .Returns(fakeGeneratorOptions);
            var service = new CodeGeneratorService(optionsMock.Object);

            // Act & Assert
            Assert.Throws<DiscountCodeSizeException>(() => service.GenerateCode(size));
        }

        [Fact]
        public void GenerateCode_ShouldThrowArgumentNullException_WhenSecretKeyIsNull()
        {
            // Arrange
            var optionsMock = new Mock<IOptions<GeneratorOptions>>();
            var fakeGeneratorOptions = new GeneratorOptions { SecretKey = null };
            optionsMock.Setup(f => f.Value)
                       .Returns(fakeGeneratorOptions);
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new CodeGeneratorService(optionsMock.Object));
        }

        [Fact]
        public void GenerateCode_ShouldThrowArgumentNullException_WhenSecretKeyIsEmpty()
        {
            // Arrange
            var optionsMock = new Mock<IOptions<GeneratorOptions>>();
            var fakeGeneratorOptions = new GeneratorOptions { SecretKey = string.Empty };
            optionsMock.Setup(f => f.Value)
                       .Returns(fakeGeneratorOptions);
            // Act & Assert
            Assert.Throws<ArgumentException>(() => new CodeGeneratorService(optionsMock.Object));
        }

        [Fact]
        public void GenerateCode_ShouldThrowArgumentNullException_WhenOptionsIsNull()
        {
            // Arrange
            IOptions<GeneratorOptions> options = null;
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new CodeGeneratorService(options));
        }

        [Fact]
        public void GenerateCodes_ShouldReturnValidCodes()
        {
            // Arrange
            var optionsMock = new Mock<IOptions<GeneratorOptions>>();

            var fakeGeneratorOptions = new GeneratorOptions { SecretKey = "test" };

            optionsMock.Setup(f => f.Value)
                       .Returns(fakeGeneratorOptions);

            var service = new CodeGeneratorService(optionsMock.Object);

            // Act
            var codes = service.GenerateCodes(1, 8);

            // Assert
            Assert.NotEmpty(codes);
            Assert.All(codes, code => Assert.Matches(@"^[A-Z0-9]{8}$", code)); // Assuming the code is 8 characters long and alphanumeric
        }

        [Theory]
        [InlineData(0)]
        [InlineData(2001)]
        public void GenerateCodes_ShouldThrowDiscountCodeCountException_WhenCountIsInvalid(int count)
        {
            // Arrange
            var optionsMock = new Mock<IOptions<GeneratorOptions>>();
            var fakeGeneratorOptions = new GeneratorOptions { SecretKey = "test" };

            optionsMock.Setup(f => f.Value)
                       .Returns(fakeGeneratorOptions);
            var service = new CodeGeneratorService(optionsMock.Object);

            // Act & Assert
            Assert.Throws<DiscountCodeCountException>(() => service.GenerateCodes(count, 8).ToList());
        }

        [Fact]
        public void GenerateCodes_ShouldAllCodesBeUnique()
        {
            // Arrange
            var optionsMock = new Mock<IOptions<GeneratorOptions>>();
            var fakeGeneratorOptions = new GeneratorOptions { SecretKey = "test" };

            optionsMock.Setup(f => f.Value)
                       .Returns(fakeGeneratorOptions);
            var service = new CodeGeneratorService(optionsMock.Object);

            // Act
            var codes = service.GenerateCodes(2_000, 8);

            // Assert
            Assert.NotEmpty(codes);
            Assert.Equal(2000, codes.Distinct().Count()); // Ensure all generated codes are unique
        }
    }
}
