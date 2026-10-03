using System.ComponentModel;
using WinFormsApp = System.Windows.Forms.Application;

namespace LuuKyCanTin.WinForms.Shell;

/// <summary>
/// The borderless overlay that covers the shell while a session is locked (key-01 D). It is shown with
/// <c>Show(owner)</c>, not modally, so every open dialog keeps its state; the other forms are disabled and enabled
/// again when the lock closes. Nothing under it is closed or recreated, so unsaved input stays intact (AC 2).
/// </summary>
public partial class LockScreenForm : Form, ILockScreenView
{
    private readonly List<Form> _disabledForms = [];
    private event EventHandler? _closed;
    private bool _isClosing;

    public LockScreenForm()
    {
        InitializeComponent();
        tmrWatch.Tick += (_, _) => DisableOtherForms();
    }

    public event EventHandler? UnlockClicked;

    public event EventHandler? SignOutClicked;

    // Explicit: Form already has a Closed event, and this one only fires for the presenter.
    event EventHandler? ILockScreenView.Closed
    {
        add => _closed += value;
        remove => _closed -= value;
    }

    public string Password => txtPassword.Text;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string Title
    {
        set
        {
            Text = value;
            lblAppTitle.Text = value;
        }
    }

    public void ShowUser(string initials, string displayName)
    {
        lblInitials.Text = initials;
        lblName.Text = displayName;
    }

    public void ShowError(string message)
    {
        bnrError.Message = message;
        // The card grew with the banner; recentre it and put the caret back in the password box.
        PerformLayout();
        LayoutOverlay();
        txtPassword.SelectAll();
        txtPassword.Focus();
    }

    public void ShowLockedMessage(string message) =>
        MessageBox.Show(this, message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);

    public bool ConfirmSignOut() =>
        MessageBox.Show(
            this,
            "Mọi dữ liệu chưa lưu sẽ bị mất. Bạn có chắc muốn đăng xuất?",
            "Đăng xuất",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning,
            MessageBoxDefaultButton.Button2) == DialogResult.Yes;

    public void CloseLock()
    {
        _isClosing = true;
        Close();
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        // OnLoad, not OnShown: Show() raises Shown only once the message loop runs, and the overlay must cover the
        // shell and disable it before anything under it can be clicked.
        PerformLayout();
        FollowOwner();
        DisableOtherForms();
        tmrWatch.Start();
        LayoutOverlay();
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        LayoutOverlay();
        txtPassword.Focus();
    }

    protected override void OnActivated(EventArgs e)
    {
        base.OnActivated(e);
        txtPassword.Focus();
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        LayoutOverlay();
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);

        // Only the presenter closes the overlay: no Alt+F4, no window menu, no owner close bypasses the lock.
        if (!_isClosing)
            e.Cancel = true;
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        base.OnFormClosed(e);
        tmrWatch.Stop();
        if (Owner is { } owner)
        {
            owner.LocationChanged -= OnOwnerChanged;
            owner.SizeChanged -= OnOwnerChanged;
        }

        EnableOtherForms();
        _closed?.Invoke(this, EventArgs.Empty);
    }

    private void OnUnlockClicked(object? sender, EventArgs e) => UnlockClicked?.Invoke(this, EventArgs.Empty);

    private void OnSignOutClicked(object? sender, LinkLabelLinkClickedEventArgs e) =>
        SignOutClicked?.Invoke(this, EventArgs.Empty);

    // The overlay covers the owner's outer bounds, title bar included, and follows it while it moves or resizes.
    private void FollowOwner()
    {
        if (Owner is not { } owner)
            return;

        Bounds = owner.Bounds;
        owner.LocationChanged += OnOwnerChanged;
        owner.SizeChanged += OnOwnerChanged;
    }

    private void OnOwnerChanged(object? sender, EventArgs e)
    {
        if (Owner is { } owner && !IsDisposed)
            Bounds = owner.Bounds;
    }

    private void DisableOtherForms()
    {
        foreach (Form form in WinFormsApp.OpenForms)
        {
            if (form == this || form.IsDisposed || !form.Enabled)
                continue;

            _disabledForms.Add(form);
            form.Enabled = false;
        }
    }

    private void EnableOtherForms()
    {
        foreach (var form in _disabledForms)
        {
            if (!form.IsDisposed)
                form.Enabled = true;
        }

        _disabledForms.Clear();
    }

    private void LayoutOverlay()
    {
        if (pnlCard.Width > 0)
            pnlCard.Location = new Point(
                (ClientSize.Width - pnlCard.Width) / 2, (ClientSize.Height - pnlCard.Height) / 2);
        if (pnlHint.Width > 0)
            pnlHint.Left = (pnlHead.Width - pnlHint.Width) / 2;
    }
}
