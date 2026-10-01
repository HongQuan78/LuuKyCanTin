using System.ComponentModel;

namespace LuuKyCanTin.WinForms.Shell;

public partial class MainForm : Form, IMainView
{
    public MainForm()
    {
        InitializeComponent();
    }

    public event EventHandler? Loaded;

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
