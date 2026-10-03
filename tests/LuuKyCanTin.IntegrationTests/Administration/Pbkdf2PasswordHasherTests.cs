using LuuKyCanTin.Infrastructure.Administration;
using Shouldly;

namespace LuuKyCanTin.IntegrationTests.Administration;

public class Pbkdf2PasswordHasherTests
{
    private readonly Pbkdf2PasswordHasher _hasher = new();

    [Fact]
    public void HashThenVerify_AcceptsTheRightPassword()
    {
        var hash = _hasher.Hash("LuuKy@2026");

        _hasher.Verify("LuuKy@2026", hash).ShouldBeTrue();
    }

    [Fact]
    public void Verify_RejectsAWrongPassword()
    {
        var hash = _hasher.Hash("LuuKy@2026");

        _hasher.Verify("luuky@2026", hash).ShouldBeFalse();
    }

    [Fact]
    public void TwoHashes_OfTheSamePassword_DifferBySalt()
    {
        _hasher.Hash("LuuKy@2026").ShouldNotBe(_hasher.Hash("LuuKy@2026"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("không-phải-hash")]
    [InlineData("PBKDF2-SHA256$600000$not-base64$also-not-base64")]
    [InlineData("PBKDF2-SHA256$0$AAAA$AAAA")]
    [InlineData("PBKDF2-SHA256$600000$AAAA")]
    [InlineData("PBKDF2-SHA512$600000$AAAA$AAAA")]
    [InlineData("PBKDF2-SHA256$100000000$AAAA$AAAA")]
    [InlineData("PBKDF2-SHA256$1000001$AAAA$AAAA")]
    public void ATamperedHash_ReturnsFalseInsteadOfThrowing(string hash)
    {
        Should.NotThrow(() => _hasher.Verify("LuuKy@2026", hash).ShouldBeFalse());
    }

    [Fact]
    public void AHugeIterationCount_IsRejectedWithoutHashing()
    {
        // 2 billion iterations would run for hours if the stored count were trusted.
        Should.CompleteIn(
            () => _hasher.Verify("LuuKy@2026", "PBKDF2-SHA256$2147483647$AAAA$AAAA").ShouldBeFalse(),
            TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void TheHash_FitsTheAuditedColumn()
    {
        _hasher.Hash("LuuKy@2026").Length.ShouldBeLessThanOrEqualTo(200);
    }

    [Fact]
    public void TheStoredFormat_KeepsTheIterations()
    {
        _hasher.Hash("LuuKy@2026").ShouldStartWith("PBKDF2-SHA256$600000$");
    }
}
