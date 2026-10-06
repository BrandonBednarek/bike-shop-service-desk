namespace BikeShop.Api.Domain.Customers;

public sealed class Customer
{
    private Customer()
    {
    }

    public int Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string Phone { get; private set; } = string.Empty;

    public string PhoneDigits { get; private set; } = string.Empty;

    public string? Email { get; private set; }

    public static Customer Create(string name, string phone, string? email) => new()
    {
        Name = name,
        Phone = phone,
        PhoneDigits = PhoneNumbers.ToDigits(phone),
        Email = email,
    };
}
