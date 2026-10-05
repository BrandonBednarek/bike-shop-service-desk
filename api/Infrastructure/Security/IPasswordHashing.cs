namespace BikeShop.Api.Infrastructure.Security;

public interface IPasswordHashing
{
    string Hash(string password);

    bool Matches(string passwordHash, string password);
}
