namespace SP02Velopack;

public sealed class MainForm : Form
{
    public MainForm(string version)
    {
        Text = $"LuuKyCanTin Spike v{version}";
        Width = 520;
        Height = 220;
        StartPosition = FormStartPosition.CenterScreen;
        var label = new Label
        {
            Text = $"Running version {version}{Environment.NewLine}Update source is passed with --source <path>.",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
        };
        Controls.Add(label);
    }
}