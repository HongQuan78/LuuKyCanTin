using System.ComponentModel;

namespace LuuKyCanTin.WinForms.HeThong;

/// <summary>
/// Shows a temporary password once, with a copy button. The value lives only in the text box; closing the
/// dialog discards it and the database holds only its hash.
/// </summary>
public partial class MatKhauTamForm : Form
{
    public MatKhauTamForm(string matKhauTam)
    {
        InitializeComponent();
        txtMatKhau.Text = matKhauTam;
        btnSaoChep.Click += OnSaoChep;
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string MatKhauTam => txtMatKhau.Text;

    private void OnSaoChep(object? sender, EventArgs e)
    {
        if (MatKhauTam.Length > 0)
            Clipboard.SetText(MatKhauTam);
    }
}
