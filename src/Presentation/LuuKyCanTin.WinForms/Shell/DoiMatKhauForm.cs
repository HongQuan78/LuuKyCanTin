using System.ComponentModel;

namespace LuuKyCanTin.WinForms.Shell;

public partial class DoiMatKhauForm : Form, IDoiMatKhauView
{
    public DoiMatKhauForm()
    {
        InitializeComponent();
        btnLuu.Click += (_, _) => LuuClicked?.Invoke(this, EventArgs.Empty);
        btnHuy.Click += (_, _) => HuyClicked?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler? LuuClicked;

    public event EventHandler? HuyClicked;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string TieuDe
    {
        set => Text = value;
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool BatBuoc
    {
        set => lblHuongDan.Text = value
            ? "Bạn phải đổi mật khẩu trước khi vào hệ thống. Ít nhất 8 ký tự, gồm chữ in hoa, chữ thường và chữ số."
            : "Ít nhất 8 ký tự, gồm chữ in hoa, chữ thường và chữ số.";
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string MatKhauHienTai => txtMatKhauHienTai.Text;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string MatKhauMoi => txtMatKhauMoi.Text;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string XacNhan => txtXacNhan.Text;

    public void HienLoi(string thongBao)
    {
        lblLoi.Text = thongBao;
        txtMatKhauHienTai.SelectAll();
        txtMatKhauHienTai.Focus();
    }

    public void DongVoiKetQua(bool thanhCong)
    {
        DialogResult = thanhCong ? DialogResult.OK : DialogResult.Cancel;
        Close();
    }

    public bool HienThi() => ShowDialog() == DialogResult.OK;

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        txtMatKhauHienTai.Focus();
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);

        // The X behaves like Hủy, so a forced change signs out before the dialog really closes. When the
        // presenter closes the dialog it has already set DialogResult, so this branch is skipped.
        if (DialogResult == DialogResult.None && e.CloseReason == CloseReason.UserClosing)
        {
            e.Cancel = true;
            HuyClicked?.Invoke(this, EventArgs.Empty);
        }
    }
}
