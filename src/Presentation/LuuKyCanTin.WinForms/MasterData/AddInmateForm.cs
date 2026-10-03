using LuuKyCanTin.Application.MasterData;
using LuuKyCanTin.Domain.MasterData;

namespace LuuKyCanTin.WinForms.MasterData;

public partial class AddInmateForm : Form, IAddInmateView
{
    private sealed record InmateTypeItem(InmateType Value)
    {
        public override string ToString() => Value.ToDisplayText();
    }

    public AddInmateForm()
    {
        InitializeComponent();
        cmbInmateType.Items.AddRange(
        [
            new InmateTypeItem(InmateType.PreTrialDetainee),
            new InmateTypeItem(InmateType.Prisoner),
        ]);
        cmbInmateType.SelectedIndex = 0;
    }

    public event EventHandler? SaveClicked;

    public string InmateCode => txtInmateCode.Text;

    public string FullName => txtFullName.Text;

    public short? BirthYear => numBirthYear.Value == 0 ? null : (short)numBirthYear.Value;

    public InmateType InmateType => ((InmateTypeItem)cmbInmateType.SelectedItem!).Value;

    public DateOnly AdmissionDate => DateOnly.FromDateTime(dtpAdmissionDate.Value);

    public string? Cell => txtCell.Text;

    public void ShowError(string message)
    {
        // The save is over; the user may correct the form and try again.
        btnSave.Enabled = true;
        MessageBox.Show(this, message, "Không lưu được", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
        SaveClicked?.Invoke(this, EventArgs.Empty);
    }
}
