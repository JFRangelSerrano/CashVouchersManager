using CashVouchersManager.Domain.Services;

namespace CashVouchersManager.Tests.Domain;

/// <summary>
/// Unit tests for the VoucherCodeGenerator service
/// </summary>
public class VoucherCodeGeneratorTests
{
    private readonly VoucherCodeGenerator _generator;

    /// <summary>
    /// Initializes a new instance of the VoucherCodeGeneratorTests class
    /// </summary>
    public VoucherCodeGeneratorTests()
    {
        _generator = new VoucherCodeGenerator();
    }

    /// <summary>
    /// Tests that generated codes have exactly 13 digits
    /// </summary>
    [Fact]
    public void GenerateCode_ShouldReturn13DigitCode()
    {
        // Arrange
        int issuingStoreId = 1234;

        // Act
        string code = _generator.GenerateCode(issuingStoreId);

        // Assert
        Assert.Equal(13, code.Length);
        Assert.All(code, c => Assert.True(char.IsDigit(c)));
    }

    /// <summary>
    /// Tests that the code starts with the store ID padded to 4 digits
    /// </summary>
    [Theory]
    [InlineData(1, "0001")]
    [InlineData(12, "0012")]
    [InlineData(123, "0123")]
    [InlineData(1234, "1234")]
    [InlineData(9999, "9999")]
    public void GenerateCode_ShouldStartWithPaddedStoreId(int storeId, string expectedPrefix)
    {
        // Act
        string code = _generator.GenerateCode(storeId);

        // Assert
        Assert.StartsWith(expectedPrefix, code);
    }

    /// <summary>
    /// Tests that the EAN13 check digit is calculated correctly
    /// </summary>
    [Fact]
    public void GenerateCode_ShouldHaveValidEan13CheckDigit()
    {
        // Arrange
        int issuingStoreId = 1234;

        // Act
        string code = _generator.GenerateCode(issuingStoreId);

        // Assert
        bool isValid = ValidateEan13CheckDigit(code);
        Assert.True(isValid, $"Generated code {code} has an invalid check digit");
    }

    /// <summary>
    /// Tests that multiple generated codes are different
    /// </summary>
    [Fact]
    public void GenerateCode_ShouldGenerateDifferentCodes()
    {
        // Arrange
        int issuingStoreId = 1234;
        var codes = new HashSet<string>();

        // Act
        for (int i = 0; i < 100; i++)
        {
            string code = _generator.GenerateCode(issuingStoreId);
            codes.Add(code);
        }

        // Assert
        Assert.True(codes.Count > 90, "Generated codes should be mostly unique");
    }

    /// <summary>
    /// Validates the EAN13 check digit of a code
    /// </summary>
    /// <param name="code">The EAN13 code to validate</param>
    /// <returns>True if the check digit is valid, false otherwise</returns>
    private bool ValidateEan13CheckDigit(string code)
    {
        if (code.Length != 13)
            return false;

        int sum = 0;
        for (int i = 0; i < 12; i++)
        {
            int digit = int.Parse(code[i].ToString());
            sum += (i % 2 == 0) ? digit : digit * 3;
        }

        int checkDigit = (sum % 10 == 0) ? 0 : (10 - (sum % 10));
        int actualCheckDigit = int.Parse(code[12].ToString());

        return checkDigit == actualCheckDigit;
    }
}
