namespace LuuKyCanTin.WinForms.Shell;

/// <summary>
/// Watches keyboard and mouse input for the whole application and raises <see cref="IdleTimeout"/> when the session
/// has been idle for the configured time. Timing uses <see cref="Environment.TickCount64"/> because it is elapsed
/// time, not a business date, so it needs no <c>IClock</c> and is immune to clock changes.
/// </summary>
public sealed class IdleMonitor : IMessageFilter, IDisposable
{
    private const int WmKeyDown = 0x0100;
    private const int WmSysKeyDown = 0x0104;
    private const int WmMouseMove = 0x0200;
    private const int WmLButtonDown = 0x0201;
    private const int WmRButtonDown = 0x0204;
    private const int WmMButtonDown = 0x0207;
    private const int WmMouseWheel = 0x020A;
    private const int CheckIntervalMilliseconds = 5000;
    private const int VirtualKeyL = 0x4C;

    private readonly Func<long> _now;
    private readonly Func<bool> _isControlDown;
    private readonly TimeSpan _timeout;
    private readonly System.Windows.Forms.Timer? _timer;
    private long _lastInput;
    private Point? _lastMousePosition;
    private bool _isPaused;
    private bool _isDisposed;

    /// <param name="timeout">The idle time that locks the session; null turns auto-lock off.</param>
    /// <param name="now">The elapsed-milliseconds source; tests replace it with a controllable value.</param>
    /// <param name="isControlDown">Whether Ctrl is held; tests replace it with a controllable value.</param>
    public IdleMonitor(TimeSpan? timeout, Func<long>? now = null, Func<bool>? isControlDown = null)
    {
        _now = now ?? (() => Environment.TickCount64);
        _isControlDown = isControlDown ?? (() => (Control.ModifierKeys & Keys.Control) == Keys.Control);
        _timeout = timeout ?? TimeSpan.Zero;
        _lastInput = _now();

        if (_timeout > TimeSpan.Zero)
        {
            _timer = new System.Windows.Forms.Timer { Interval = CheckIntervalMilliseconds };
            _timer.Tick += OnTick;
        }
    }

    /// <summary>Raised once on the UI thread when the idle time has passed; the listener locks the session.</summary>
    public event EventHandler? IdleTimeout;

    /// <summary>
    /// Raised on Ctrl+L. The filter sees keys for modal dialogs too, where the shell's ProcessCmdKey never runs,
    /// so the shortcut works whatever dialog has the focus.
    /// </summary>
    public event EventHandler? LockRequested;

    /// <summary>
    /// The pure idle decision: true when the timeout has passed with no input since <paramref name="lastInput"/>.
    /// A paused monitor never locks, and a zero timeout turns auto-lock off.
    /// </summary>
    public static bool ShouldLock(long lastInput, long now, TimeSpan timeout, bool isPaused) =>
        !isPaused && timeout > TimeSpan.Zero && now - lastInput >= (long)timeout.TotalMilliseconds;

    public void Start() => _timer?.Start();

    /// <summary>Stops the check while the app is already locked or a long operation runs.</summary>
    public void Pause() => _isPaused = true;

    /// <summary>Restarts the idle clock from now, so the time spent paused never counts as idle.</summary>
    public void Resume()
    {
        _lastInput = _now();
        _isPaused = false;
    }

    public bool PreFilterMessage(ref Message m)
    {
        if (_isPaused)
            return false;

        if (m.Msg == WmKeyDown && m.WParam.ToInt32() == VirtualKeyL && _isControlDown())
        {
            LockRequested?.Invoke(this, EventArgs.Empty);
            return true;
        }

        if (!IsInputMessage(m.Msg))
            return false;

        // Some drivers send phantom WM_MOUSEMOVE messages; a move that didn't move is not activity.
        if (m.Msg == WmMouseMove)
        {
            var position = GetMousePosition(m);
            if (position == _lastMousePosition)
                return false;
            _lastMousePosition = position;
        }

        _lastInput = _now();
        return false;
    }

    public void Dispose()
    {
        if (_isDisposed)
            return;
        _isDisposed = true;
        _timer?.Dispose();
    }

    private static bool IsInputMessage(int message) => message is
        WmKeyDown or WmSysKeyDown or WmMouseMove or WmLButtonDown or WmRButtonDown or WmMButtonDown or WmMouseWheel;

    private static Point GetMousePosition(Message m)
    {
        var value = m.LParam.ToInt64();
        return new Point(unchecked((short)(value & 0xFFFF)), unchecked((short)((value >> 16) & 0xFFFF)));
    }

    private void OnTick(object? sender, EventArgs e) => CheckIdle();

    /// <summary>The check the timer runs, exposed so tests can drive it without a message loop.</summary>
    internal void CheckIdle()
    {
        if (!ShouldLock(_lastInput, _now(), _timeout, _isPaused))
            return;

        // Pause until the listener resumes after unlock; without it every tick would raise the event again.
        Pause();
        IdleTimeout?.Invoke(this, EventArgs.Empty);
    }
}
