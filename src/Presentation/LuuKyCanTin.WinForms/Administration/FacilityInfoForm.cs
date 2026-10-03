using LuuKyCanTin.Application.Administration;
using LuuKyCanTin.WinForms.Common;

namespace LuuKyCanTin.WinForms.Administration;

/// <summary>The unit-information screen, hosted in the shell's content area: the three header lines and a print preview.</summary>
public partial class FacilityInfoForm : UserControl, IFacilityInfoView
{
    private readonly FieldErrorDisplay<FacilityInfoField> _fieldErrors = new();

    public FacilityInfoForm()
    {
        InitializeComponent();
        txtParentAgency.MaxLength = SaveFacilityInfoRequestValidator.ParentAgencyNameMaxLength;
        txtFacilityName.MaxLength = SaveFacilityInfoRequestValidator.FacilityNameMaxLength;
        txtAddress.MaxLength = SaveFacilityInfoRequestValidator.AddressMaxLength;

        _fieldErrors.Add(FacilityInfoField.ParentAgencyName, frmParentAgency, errParentAgency);
        _fieldErrors.Add(FacilityInfoField.FacilityName, frmFacilityName, errFacilityName);
        _fieldErrors.Add(FacilityInfoField.Address, frmAddress, errAddress);

        txtParentAgency.TextChanged += (_, _) => UpdatePreview();
        txtFacilityName.TextChanged += (_, _) => UpdatePreview();
        txtAddress.TextChanged += (_, _) => UpdatePreview();

        btnSave.Click += (_, _) =>
        {
            // The new attempt replaces the old messages; the presenter shows whatever is still wrong.
            _fieldErrors.ClearAll();
            bnrMessage.Message = "";
            SaveClicked?.Invoke(this, EventArgs.Empty);
        };
        btnReset.Click += (_, _) =>
        {
            _fieldErrors.ClearAll();
            bnrMessage.Message = "";
            ReloadClicked?.Invoke(this, EventArgs.Empty);
        };
    }

    public event EventHandler? Loaded;

    public event EventHandler? SaveClicked;

    public event EventHandler? ReloadClicked;

    public string ParentAgencyName => txtParentAgency.Text;

    public string FacilityName => txtFacilityName.Text;

    public string Address => txtAddress.Text;

    public void ShowFacility(FacilityInfoDto facility)
    {
        txtParentAgency.Text = facility.ParentAgencyName ?? "";
        txtFacilityName.Text = facility.FacilityName;
        txtAddress.Text = facility.Address;
        UpdatePreview();
    }

    public void ShowFieldErrors(IReadOnlyList<FieldMessage<FacilityInfoField>> errors) => _fieldErrors.Show(errors);

    public void ShowMessage(string message)
    {
        bnrMessage.Kind = BannerKind.Warning;
        bnrMessage.Message = message;
    }

    public void ShowError(string message)
    {
        bnrMessage.Kind = BannerKind.Error;
        bnrMessage.Message = message;
    }

    public void SetEditingEnabled(bool canEdit)
    {
        frmParentAgency.ReadOnly = !canEdit;
        frmFacilityName.ReadOnly = !canEdit;
        frmAddress.ReadOnly = !canEdit;
        btnSave.Visible = canEdit;
        btnReset.Visible = canEdit;
        if (!canEdit)
            _fieldErrors.ClearAll();
    }

    public void SetSaveEnabled(bool isEnabled) => btnSave.Enabled = isEnabled;

    // A cached screen loads once, the first time the shell shows it.
    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        Loaded?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>The header exactly as the printed templates stack it: agency in capitals when set, then name, then address.</summary>
    private void UpdatePreview()
    {
        var lines = new List<string>();
        if (!string.IsNullOrWhiteSpace(txtParentAgency.Text))
            lines.Add(FacilityHeaderLines.FormatParentAgency(txtParentAgency.Text));
        if (!string.IsNullOrWhiteSpace(txtFacilityName.Text))
            lines.Add(FacilityHeaderLines.FormatFacilityName(txtFacilityName.Text));
        if (!string.IsNullOrWhiteSpace(txtAddress.Text))
            lines.Add(FacilityHeaderLines.FormatAddressLine(txtAddress.Text));

        lblPreview.Text = string.Join(Environment.NewLine, lines);
    }
}
