using System.ComponentModel;

namespace LuuKyCanTin.WinForms.DanhMuc;

public partial class CanBoEditForm : Form, ICanBoEditView
{
    public CanBoEditForm()
    {
        InitializeComponent();
        btnLuu.Click += (_, _) => LuuClicked?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler? LuuClicked;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string TieuDe
    {
        set => Text = value;
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string MaCanBo
    {
        get => txtMaCanBo.Text;
        set => txtMaCanBo.Text = value;
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string HoTen
    {
        get => txtHoTen.Text;
        set => txtHoTen.Text = value;
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string ChucVu
    {
        get => txtChucVu.Text;
        set => txtChucVu.Text = value;
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool LaQuanGiao
    {
        get => chkLaQuanGiao.Checked;
        set => chkLaQuanGiao.Checked = value;
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool DangCongTac
    {
        get => chkDangCongTac.Checked;
        set => chkDangCongTac.Checked = value;
    }

    public bool HienThi() => ShowDialog() == DialogResult.OK;

    public void HienLoi(string thongBao)
    {
        MessageBox.Show(this, thongBao, "Không lưu được", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    public void DongDaLuu()
    {
        DialogResult = DialogResult.OK;
    }
}
