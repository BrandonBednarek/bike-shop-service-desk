namespace BikeShop.Api.Domain.Customers;

public static class PhoneNumbers
{
    private const char NorthAmericanCountryCode = '1';

    private const int DigitsIncludingCountryCode = 11;

    public static string ToDigits(string phone) => WithoutCountryCode(string.Concat(phone.Where(char.IsAsciiDigit)));

    private static string WithoutCountryCode(string digits) =>
        digits.Length == DigitsIncludingCountryCode && digits[0] == NorthAmericanCountryCode ? digits[1..] : digits;
}
