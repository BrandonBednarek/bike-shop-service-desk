using BikeShop.Api.Infrastructure.Security;

using Shouldly;

namespace BikeShop.Api.Tests;

public sealed class IdentityPasswordHashingTests
{
    private readonly IdentityPasswordHashing _hashing = new();

    [Fact]
    public void AHashMatchesThePasswordItWasMadeFrom()
    {
        string hash = _hashing.Hash("correct horse");

        _hashing.Matches(hash, "correct horse").ShouldBeTrue();
    }

    [Fact]
    public void AHashDoesNotMatchADifferentPassword()
    {
        string hash = _hashing.Hash("correct horse");

        _hashing.Matches(hash, "battery staple").ShouldBeFalse();
    }

    [Fact]
    public void HashingTheSamePasswordTwiceGivesDifferentHashes()
    {
        string first = _hashing.Hash("correct horse");
        string second = _hashing.Hash("correct horse");

        first.ShouldNotBe(second, "each hash should use its own random salt");
    }
}
