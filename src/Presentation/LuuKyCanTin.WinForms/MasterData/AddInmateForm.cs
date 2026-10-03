using LuuKyCanTin.Application.MasterData;
using LuuKyCanTin.Domain.MasterData;
using LuuKyCanTin.WinForms.Common;

namespace LuuKyCanTin.WinForms.MasterData;

public partial class AddInmateForm : Form, IAddInmateView
{
    private sealed record InmateTypeItem(InmateType Value)
    {
        public override string ToString() => Value.ToDisplayText();
    }

    private readonly FieldErrorDisplay<InmateField> _fieldErrors = new();

    public AddInmateForm()
    {
        InitializeComponent();
        cmbInmateType.Items.AddRange(
        [
            new InmateTypeItem(InmateType.PreTrialDetainee),
            new InmateTypeItem(InmateType.Prisoner),
        ]);
        cmbInmateType.SelectedIndex = 0;
        _fieldErrors.Add(InmateField.InmateCode, frmInmateCode, errInmateCode);
        _fieldErrors.Add(InmateField.BirthYear, frmBirthYear, errBirthYear);
        _fieldErrors.Add(InmateField.FullName, frmFullName, errFullName);
        _fieldErrors.Add(InmateField.InmateType, frmInmateType, errInmateType);
        _fieldErrors.Add(InmateField.AdmissionDate, frmAdmissionDate, errAdmissionDate);
        _fieldErrors.Add(InmateField.Cell, frmCell, errCell);
    }

    public event EventHandler? SaveClicked;

    public string InmateCode => txtInmateCode.Text;

    public string FullName => txtFullName.Text;

    public short? BirthYear => numBirthYear.Value == 0 ? null : (short)numBirthYear.Value;

    public InmateType InmateType => ((InmateTypeItem)cmbInmateType.SelectedItem!).Value;

    public DateOnly AdmissionDate => DateOnly.FromDateTime(dtpAdmissionDate.Value);

    public string? Cell => txtCell.Text;

    public void ShowFieldErrors(IReadOnlyList<FieldMessage<InmateField>> errors)
    {
        // The save is over; the user may correct the form and try again.
        btnSave.Enabled = true;
        _fieldErrors.Show(errors);
    }

    public void ShowError(string message)
    {
        btnSave.Enabled = true;
        bnrError.Message = message;
    }

    public void CloseWithResult(bool succeeded)
    {
        DialogResult = succeeded ? DialogResult.OK : DialogResult.Cancel;
        Close();
    }

    private void OnSaveClicked(object? sender, EventArgs e)
    {
        // Closing the window is instant; without this a double-click starts a second save.
        btnSave.Enabled = false;
        _fieldErrors.ClearAll();
        bnrError.Message = "";
        SaveClicked?.Invoke(this, EventArgs.Empty);
    }
}
