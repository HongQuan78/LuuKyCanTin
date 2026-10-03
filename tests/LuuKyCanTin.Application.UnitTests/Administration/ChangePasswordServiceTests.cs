using LuuKyCanTin.Application.Abstractions;
using LuuKyCanTin.Application.Administration;
using LuuKyCanTin.Application.Common;
using LuuKyCanTin.Application.UnitTests.TestUtilities;
using LuuKyCanTin.Domain.Administration;
using NSubstitute;
using Shouldly;

namespace LuuKyCanTin.Application.UnitTests.Administration;

public class ChangePasswordServiceTests
{
    private const string OldHash = "PBKDF2-SHA256$1$cu$cu";
    private static readonly DateTime Now = new(2026, 10, 2, 9, 0, 0);

    private readonly IUserStore _store = Substitute.For<IUserStore>();
    private readonly IPasswordHasher _hasher = Substitute.For<IPasswordHasher>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IAuditLogWriter _auditLog = Substitute.For<IAuditLogWriter>();
    private readonly SignInOptions _options = new() { LockoutMinutes = 15 };
    private readonly ChangePasswordService _service;
    private User _user = null!;

    public ChangePasswordServiceTests()
    {
        var failedSignIns = new FailedSignInService(_store, new FakeClock(Now), _options, _auditLog);
        _service = new ChangePasswordService(_store, _hasher, _currentUser, _auditLog, failedSignIns);
    }

    private void SeedUser(bool mustChangePassword = false)
    {
        _currentUser.UserId.Returns(7);
        _user = new User
        {
            Id = 7,
            UserName = "luuky",
            PasswordHash = OldHash,
            IsActive = true,
            MustChangePassword = mustChangePassword,
        };
        _store.FindByIdAsync(7, Arg.Any<CancellationToken>()).Returns(_user);
        _store.SaveAsync(_user, Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task ChangePassword_WithAValidNewPassword_StoresTheHashAndLogs()
    {
        SeedUser(mustChangePassword: true);
        _hasher.Verify("LuuKy@2026", OldHash).Returns(true);
        _hasher.Hash("Moi@2026a").Returns("hash-moi");

        await _service.ChangePasswordAsync("LuuKy@2026", "Moi@2026a", "Moi@2026a");

        _user.PasswordHash.ShouldBe("hash-moi");
        _user.MustChangePassword.ShouldBeFalse();
        await _store.Received(1).SaveAsync(_user, Arg.Any<CancellationToken>());
        await _auditLog.Received(1).WriteAsync(
            AuditAction.SignIn, "User", 7,
            Arg.Is<object?>(o => o!.ToString()!.Contains($"Event = {SignInEvent.PasswordChanged}")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ChangePassword_WithAWrongCurrentPassword_CountsTowardLockout()
    {
        SeedUser();
        _hasher.Verify("sai", OldHash).Returns(false);

        var error = await Should.ThrowAsync<BusinessRuleException>(
            () => _service.ChangePasswordAsync("sai", "Moi@2026a", "Moi@2026a"));

        error.Message.ShouldBe(SignInService.InvalidCredentialsMessage);
        _user.FailedAttemptCount.ShouldBe((byte)1);
        await _store.Received(1).SaveAsync(_user, Arg.Any<CancellationToken>());
        _hasher.DidNotReceive().Hash(Arg.Any<string>());
    }

    [Fact]
    public async Task ChangePassword_WithAWeakPassword_ReportsEveryBrokenRuleAndSavesNothing()
    {
        SeedUser();
        _hasher.Verify("LuuKy@2026", OldHash).Returns(true);

        var error = await Should.ThrowAsync<BusinessRuleException>(
            () => _service.ChangePasswordAsync("LuuKy@2026", "abc", "abc"));

        error.Message.ShouldContain(PasswordPolicy.TooShortMessage);
        error.Message.ShouldContain(PasswordPolicy.MissingUpperCaseMessage);
        error.Message.ShouldContain(PasswordPolicy.MissingDigitMessage);
        await _store.DidNotReceive().SaveAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ChangePassword_WithTheCurrentPassword_IsRejected()
    {
        SeedUser();
        _hasher.Verify("LuuKy@2026", OldHash).Returns(true);

        var error = await Should.ThrowAsync<BusinessRuleException>(
            () => _service.ChangePasswordAsync("LuuKy@2026", "LuuKy@2026", "LuuKy@2026"));

        error.Message.ShouldBe(PasswordPolicy.SameAsCurrentMessage);
    }

    [Fact]
    public async Task ChangePassword_WithAMismatchedConfirmation_IsRejected()
    {
        SeedUser();
        _hasher.Verify("LuuKy@2026", OldHash).Returns(true);

        var error = await Should.ThrowAsync<BusinessRuleException>(
            () => _service.ChangePasswordAsync("LuuKy@2026", "Moi@2026a", "Khac@2026a"));

        error.Message.ShouldBe(PasswordPolicy.ConfirmationMismatchMessage);
    }

    [Fact]
    public async Task ChangePassword_WithoutASignedInUser_IsRejected()
    {
        _currentUser.UserId.Returns((int?)null);

        var error = await Should.ThrowAsync<BusinessRuleException>(
            () => _service.ChangePasswordAsync("a", "Moi@2026a", "Moi@2026a"));

        error.Message.ShouldBe(ChangePasswordService.NotSignedInMessage);
    }
}
