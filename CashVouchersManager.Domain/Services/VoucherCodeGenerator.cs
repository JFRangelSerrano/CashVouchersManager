namespace CashVouchersManager.Domain.Services;

/// <summary>
/// Service for generating EAN13 voucher codes
/// </summary>
public class VoucherCodeGenerator
{
    private readonly Random _random = new Random();

    /// <summary>
    /// Generates an EAN13 code for a voucher
    /// </summary>
    /// <param name="issuingStoreId">The issuing store ID (max 4 digits)</param>
    /// <returns>A valid EAN13 code</returns>
    public string GenerateCode(int issuingStoreId)
    {
        // Store ID padded to 4 digits
        string storeCode = issuingStoreId.ToString().PadLeft(4, '0');
        
        // Generate random sequence to complete 12 digits
        string randomSequence = GenerateRandomSequence(8);
        
        // Combine store code and random sequence (12 digits total)
        string code12 = storeCode + randomSequence;
        
        // Calculate check digit
        int checkDigit = CalculateEan13CheckDigit(code12);
        
        return code12 + checkDigit.ToString();
    }

    /// <summary>
    /// Generates a random numeric sequence of specified length
    /// </summary>
    /// <param name="length">The length of the sequence</param>
    /// <returns>A random numeric string</returns>
    private string GenerateRandomSequence(int length)
    {
        string result = "";
        for (int i = 0; i < length; i++)
        {
            result += _random.Next(0, 10).ToString();
        }
        return result;
    }

    /// <summary>
    /// Calculates the EAN13 check digit
    /// </summary>
    /// <param name="code12">The first 12 digits of the code</param>
    /// <returns>The check digit (0-9)</returns>
    private int CalculateEan13CheckDigit(string code12)
    {
        int sum = 0;
        for (int i = 0; i < 12; i++)
        {
            int digit = int.Parse(code12[i].ToString());
            // Multiply odd positions (1-based) by 1, even positions by 3
            sum += (i % 2 == 0) ? digit : digit * 3;
        }
        
        int remainder = sum % 10;
        return (remainder == 0) ? 0 : (10 - remainder);
    }
}
