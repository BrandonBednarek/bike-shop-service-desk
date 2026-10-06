using BikeShop.Api.Domain.Customers;

using Shouldly;

namespace BikeShop.Api.Tests.Domain;

public sealed class CustomerTests
{
    [Fact]
    public void APhoneNumberTypedDifferentWaysIsStoredAsTheSameDigits()
    {
        string[] sameNumberTypedThreeWays = ["(416) 123-4567", "416.123.4567", "+1 416 123 4567"];

        IEnumerable<string> storedDigits = sameNumberTypedThreeWays
            .Select(phone => Customer.Create("Henry Fonda", phone, email: null).PhoneDigits);

        storedDigits.ShouldAllBe(digits => digits == "4161234567");
    }
}
