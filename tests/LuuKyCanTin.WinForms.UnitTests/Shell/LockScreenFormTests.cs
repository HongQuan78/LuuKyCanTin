using LuuKyCanTin.WinForms.Shell;
using LuuKyCanTin.WinForms.UnitTests.TestUtilities;
using Shouldly;

namespace LuuKyCanTin.WinForms.UnitTests.Shell;

/// <summary>
/// The overlay's cover/disable behaviour touches <c>Application.OpenForms</c>, which is process-global. Its
/// collection disables parallelization so no other test's form is disabled while one of these runs.
/// </summary>
[Collection(LockScreenOverlayCollection.Name)]
public class LockScreenFormTests
{
    [Fact]
    public void Show_DisablesTheOwnerAndCloseLockReenablesIt()
    {
        StaThread.Run(() =>
        {
            using var owner = new Form { Opacity = 0, ShowInTaskbar = false };
            owner.Show();
            using var lockForm = new LockScreenForm { Opacity = 0 };
            lockForm.Show(owner);

            lockForm.Bounds.ShouldBe(owner.Bounds);
            owner.Enabled.ShouldBeFalse();

            ((ILockScreenView)lockForm).CloseLock();

            owner.Enabled.ShouldBeTrue();
            lockForm.IsDisposed.ShouldBeTrue();
        });
    }

    [Fact]
    public void Close_WithoutThePresenter_IsCancelled()
    {
        StaThread.Run(() =>
        {
            using var owner = new Form { Opacity = 0, ShowInTaskbar = false };
            owner.Show();
            using var lockForm = new LockScreenForm { Opacity = 0 };
            lockForm.Show(owner);

            lockForm.Close();

            lockForm.IsDisposed.ShouldBeFalse();
            owner.Enabled.ShouldBeFalse();
        });
    }
}

[CollectionDefinition(LockScreenOverlayCollection.Name, DisableParallelization = true)]
public sealed class LockScreenOverlayCollection
{
    public const string Name = "LockScreenOverlay";
}
