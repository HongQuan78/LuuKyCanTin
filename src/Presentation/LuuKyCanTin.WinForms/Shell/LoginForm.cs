namespace LuuKyCanTin.WinForms.Shell;

public partial class LoginForm : Form, ILoginView
{
    public LoginForm()
    {
        InitializeComponent();
    }

    public event EventHandler? DangNhapBam;

    public string TenDangNhap => txtTenDangNhap.Text;

    public string MatKhau => txtMatKhau.Text;

    public void HienLoi(string thongBao)
    {
        lblLoi.Text = thongBao;
        txtMatKhau.SelectAll();
        txtMatKhau.Focus();
    }

    public void XoaMatKhau()
    {
        txtMatKhau.Clear();
        txtMatKhau.Focus();
    }

    public void DongVoiKetQua(bool thanhCong)
    {
        DialogResult = thanhCong ? DialogResult.OK : DialogResult.Cancel;
        Close();
    }

    private void OnDangNhapBam(object? sender, EventArgs e) => DangNhapBam?.Invoke(this, EventArgs.Empty);

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        txtTenDangNhap.Focus();
    }
}
