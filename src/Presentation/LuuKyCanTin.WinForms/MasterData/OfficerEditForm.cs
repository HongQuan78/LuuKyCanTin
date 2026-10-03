using System.ComponentModel;

namespace LuuKyCanTin.WinForms.MasterData;

public partial class OfficerEditForm : Form, IOfficerEditView
{
    public OfficerEditForm()
    {
        InitializeComponent();
        btnSave.Click += (_, _) => SaveClicked?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler? SaveClicked;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string Title
    {
        set => Text = value;
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string OfficerCode
    {
        get => txtOfficerCode.Text;
        set => txtOfficerCode.Text = value;
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string FullName
    {
        get => txtFullName.Text;
        set => txtFullName.Text = value;
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string Position
    {
        get => txtPosition.Text;
        set => txtPosition.Text = value;
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool IsSupervisingOfficer
    {
        get => chkIsSupervisingOfficer.Checked;
        set => chkIsSupervisingOfficer.Checked = value;
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool IsActive
    {
        get => chkIsActive.Checked;
        set => chkIsActive.Checked = value;
    }

    public bool DisplayText() => ShowDialog() == DialogResult.OK;

    public void ShowError(string message)
    {
        MessageBox.Show(this, message, "Không lưu được", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    public void CloseAsSaved()
    {
        DialogResult = DialogResult.OK;
    }
}
