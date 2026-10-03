using LuuKyCanTin.WinForms.Shell;
using Shouldly;

namespace LuuKyCanTin.WinForms.UnitTests.Shell;

/// <summary>The pure idle decision, the input filter and its 5-second timer.</summary>
public class IdleMonitorTests
{
    private const int WmKeyDown = 0x0100;
    private const int WmMouseMove = 0x0200;
    private static readonly TimeSpan FiveMinutes = TimeSpan.FromMinutes(5);

    [Fact]
    public void ShouldLock_TimeoutElapsed_Locks()
    {
        IdleMonitor.ShouldLock(1_000, 1_000 + 300_000, FiveMinutes, isPaused: false).ShouldBeTrue();
    }

    [Fact]
    public void ShouldLock_BeforeTheTimeout_DoesNotLock()
    {
        IdleMonitor.ShouldLock(1_000, 1_000 + 299_999, FiveMinutes, isPaused: false).ShouldBeFalse();
    }

    [Fact]
    public void ShouldLock_Paused_DoesNotLock()
    {
        IdleMonitor.ShouldLock(1_000, 1_000 + 600_000, FiveMinutes, isPaused: true).ShouldBeFalse();
    }

    [Fact]
    public void ShouldLock_ZeroTimeout_AutoLockIsOff()
    {
        IdleMonitor.ShouldLock(1_000, 1_000 + 600_000, TimeSpan.Zero, isPaused: false).ShouldBeFalse();
    }

    [Fact]
    public void PreFilterMessage_KeyDown_ResetsTheIdleClock()
    {
        var now = 0L;
        using var monitor = new IdleMonitor(FiveMinutes, () => now);
        var timedOut = false;
        monitor.IdleTimeout += (_, _) => timedOut = true;

        now = 4 * 60_000;
        var message = KeyDown();
        monitor.PreFilterMessage(ref message).ShouldBeFalse();

        now = 8 * 60_000; // four minutes since the key: not idle yet
        monitor.CheckIdle();
        timedOut.ShouldBeFalse();

        now = 9 * 60_000; // five minutes since the key
        monitor.CheckIdle();
        timedOut.ShouldBeTrue();
    }

    [Fact]
    public void PreFilterMessage_PhantomMouseMove_DoesNotCountAsActivity()
    {
        var now = 0L;
        using var monitor = new IdleMonitor(FiveMinutes, () => now);
        var timedOut = false;
        monitor.IdleTimeout += (_, _) => timedOut = true;

        now = 4 * 60_000;
        var move = MouseMove(10, 10);
        monitor.PreFilterMessage(ref move);

        // The same position five minutes later is a phantom move and must not reset the clock.
        now = 9 * 60_000;
        var phantom = MouseMove(10, 10);
        monitor.PreFilterMessage(ref phantom);
        monitor.CheckIdle();

        timedOut.ShouldBeTrue();
    }

    [Fact]
    public void Pause_IgnoresInputAndResumeRestartsTheClock()
    {
        var now = 0L;
        using var monitor = new IdleMonitor(FiveMinutes, () => now);
        var timedOut = 0;
        monitor.IdleTimeout += (_, _) => timedOut++;

        monitor.Pause();
        now = 10 * 60_000;
        var message = KeyDown();
        monitor.PreFilterMessage(ref message);
        monitor.CheckIdle();
        timedOut.ShouldBe(0);

        monitor.Resume();
        monitor.CheckIdle();
        timedOut.ShouldBe(0);

        now += 5 * 60_000;
        monitor.CheckIdle();
        timedOut.ShouldBe(1);
    }

    [Fact]
    public void CheckIdle_AfterTheTimeout_RaisesOnceUntilResumed()
    {
        var now = 0L;
        using var monitor = new IdleMonitor(FiveMinutes, () => now);
        var timedOut = 0;
        monitor.IdleTimeout += (_, _) => timedOut++;

        now = 5 * 60_000;
        monitor.CheckIdle();
        monitor.CheckIdle();
        monitor.CheckIdle();

        timedOut.ShouldBe(1);
    }

    [Fact]
    public void PreFilterMessage_CtrlL_RaisesLockRequestedAndConsumesTheKey()
    {
        using var monitor = new IdleMonitor(FiveMinutes, () => 0, isControlDown: () => true);
        var lockCount = 0;
        monitor.LockRequested += (_, _) => lockCount++;

        var message = Message.Create(IntPtr.Zero, WmKeyDown, (IntPtr)Keys.L, IntPtr.Zero);

        monitor.PreFilterMessage(ref message).ShouldBeTrue();
        lockCount.ShouldBe(1);
    }

    [Fact]
    public void PreFilterMessage_CtrlL_WhenPaused_DoesNothing()
    {
        using var monitor = new IdleMonitor(FiveMinutes, () => 0, isControlDown: () => true);
        var lockCount = 0;
        monitor.LockRequested += (_, _) => lockCount++;
        monitor.Pause();

        var message = Message.Create(IntPtr.Zero, WmKeyDown, (IntPtr)Keys.L, IntPtr.Zero);

        monitor.PreFilterMessage(ref message).ShouldBeFalse();
        lockCount.ShouldBe(0);
    }

    private static Message KeyDown() => Message.Create(IntPtr.Zero, WmKeyDown, (IntPtr)Keys.A, IntPtr.Zero);

    private static Message MouseMove(int x, int y) =>
        Message.Create(IntPtr.Zero, WmMouseMove, IntPtr.Zero, (IntPtr)((y << 16) | (x & 0xFFFF)));
}
