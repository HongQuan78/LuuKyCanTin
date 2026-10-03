using LuuKyCanTin.Domain.Administration;
using Shouldly;

namespace LuuKyCanTin.Domain.UnitTests.Administration;

public class UserTests
{
    private static readonly DateTime Now = new(2026, 10, 2, 9, 0, 0);
    private static readonly TimeSpan FifteenMinutes = TimeSpan.FromMinutes(15);

    private static User Account() => new() { Id = 1, UserName = "admin" };

    [Fact]
    public void RecordFailedSignIn_FourthAttempt_DoesNotLock()
    {
        var user = Account();

        for (var attempt = 0; attempt < User.MaxFailedAttempts - 1; attempt++)
            user.RecordFailedSignIn(Now, FifteenMinutes).ShouldBeFalse();

        user.FailedAttemptCount.ShouldBe((byte)4);
        user.LockedUntil.ShouldBeNull();
    }

    [Fact]
    public void RecordFailedSignIn_FifthAttempt_LocksForTheConfiguredPeriod()
    {
        var user = Account();
        for (var attempt = 0; attempt < User.MaxFailedAttempts - 1; attempt++)
            user.RecordFailedSignIn(Now, FifteenMinutes);

        var justLocked = user.RecordFailedSignIn(Now, FifteenMinutes);

        justLocked.ShouldBeTrue();
        user.FailedAttemptCount.ShouldBe(User.MaxFailedAttempts);
        user.LockedUntil.ShouldBe(Now + FifteenMinutes);
        user.IsLocked(Now).ShouldBeTrue();
    }

    [Fact]
    public void RecordFailedSignIn_WithoutAPeriod_LocksUntilAnAdministratorUnlocks()
    {
        var user = Account();
        for (var attempt = 0; attempt < User.MaxFailedAttempts - 1; attempt++)
            user.RecordFailedSignIn(Now, lockPeriod: null);

        user.RecordFailedSignIn(Now, lockPeriod: null).ShouldBeTrue();

        user.LockedUntil.ShouldBe(new DateTime(9999, 12, 31));
        user.IsLocked(Now.AddYears(100)).ShouldBeTrue();
    }

    [Fact]
    public void IsLocked_WhenTheLockExpired_IsOpenAgain()
    {
        var user = Account();
        for (var attempt = 0; attempt < User.MaxFailedAttempts; attempt++)
            user.RecordFailedSignIn(Now, FifteenMinutes);

        user.IsLocked(Now.AddMinutes(16)).ShouldBeFalse();
    }

    [Fact]
    public void IsLocked_WithoutALock_IsOpen()
    {
        Account().IsLocked(Now).ShouldBeFalse();
    }

    [Fact]
    public void RecordSuccessfulSignIn_ResetsTheFailuresAndTheLock()
    {
        var user = Account();
        for (var attempt = 0; attempt < User.MaxFailedAttempts; attempt++)
            user.RecordFailedSignIn(Now, FifteenMinutes);

        user.RecordSuccessfulSignIn();

        user.FailedAttemptCount.ShouldBe((byte)0);
        user.LockedUntil.ShouldBeNull();
    }

    [Fact]
    public void ChangePassword_StoresTheNewHashAndClearsTheForcedChange()
    {
        var user = Account();
        user.MustChangePassword = true;

        user.ChangePassword("PBKDF2-SHA256$600000$moi$moi");

        user.PasswordHash.ShouldBe("PBKDF2-SHA256$600000$moi$moi");
        user.MustChangePassword.ShouldBeFalse();
    }
}
