using System.ComponentModel;

namespace LuuKyCanTin.WinForms.Shell;

public partial class MainForm : Form, IMainView
{
    public MainForm()
    {
        InitializeComponent();
        mnuCanBo.Click += (_, _) => DanhMucCanBoClicked?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler? Loaded;

    public event EventHandler? DanhMucCanBoClicked;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string TieuDe
    {
        set => Text = value;
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        Loaded?.Invoke(this, EventArgs.Empty);
    }
}
