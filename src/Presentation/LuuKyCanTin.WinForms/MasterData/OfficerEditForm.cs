using System.ComponentModel;
using LuuKyCanTin.WinForms.Common;

namespace LuuKyCanTin.WinForms.MasterData;

public partial class OfficerEditForm : Form, IOfficerEditView
{
    private readonly FieldErrorDisplay<OfficerField> _fieldErrors = new();

    public OfficerEditForm()
    {
        InitializeComponent();
        _fieldErrors.Add(OfficerField.OfficerCode, frmOfficerCode, errOfficerCode);
        _fieldErrors.Add(OfficerField.Position, frmPosition, errPosition);
        _fieldErrors.Add(OfficerField.FullName, frmFullName, errFullName);
        btnSave.Click += (_, _) =>
        {
            // The new attempt replaces the old messages; the presenter shows whatever is still wrong.
            _fieldErrors.ClearAll();
            bnrError.Message = "";
            SaveClicked?.Invoke(this, EventArgs.Empty);
        };
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

    public void ShowHeading(string heading, string subtitle)
    {
        lblHeading.Text = heading;
        lblSubtitle.Text = subtitle;
    }

    public bool ShowModal() => ShowDialog() == DialogResult.OK;

    public void ShowFieldErrors(IReadOnlyList<FieldMessage<OfficerField>> errors) => _fieldErrors.Show(errors);

    public void ShowError(string message) => bnrError.Message = message;

    public void CloseAsSaved()
    {
        DialogResult = DialogResult.OK;
    }
}
