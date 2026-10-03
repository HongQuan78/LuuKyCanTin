namespace LuuKyCanTin.WinForms.MasterData;

partial class OfficerEditForm
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null!;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        lblOfficerCode = new Label();
        txtOfficerCode = new TextBox();
        lblFullName = new Label();
        txtFullName = new TextBox();
        lblPosition = new Label();
        txtPosition = new TextBox();
        chkIsSupervisingOfficer = new CheckBox();
        chkIsActive = new CheckBox();
        btnSave = new Button();
        btnCancel = new Button();
        SuspendLayout();
        //
        // lblOfficerCode
        //
        lblOfficerCode.AutoSize = true;
        lblOfficerCode.Location = new Point(12, 15);
        lblOfficerCode.Name = "lblOfficerCode";
        lblOfficerCode.Text = "Mã cán bộ:";
        //
        // txtOfficerCode
        //
        txtOfficerCode.CharacterCasing = CharacterCasing.Upper;
        txtOfficerCode.Location = new Point(100, 12);
        txtOfficerCode.MaxLength = 20;
        txtOfficerCode.Name = "txtOfficerCode";
        txtOfficerCode.Size = new Size(160, 23);
        txtOfficerCode.TabIndex = 0;
        //
        // lblFullName
        //
        lblFullName.AutoSize = true;
        lblFullName.Location = new Point(12, 44);
        lblFullName.Name = "lblFullName";
        lblFullName.Text = "Họ tên:";
        //
        // txtFullName
        //
        txtFullName.Location = new Point(100, 41);
        txtFullName.MaxLength = 100;
        txtFullName.Name = "txtFullName";
        txtFullName.Size = new Size(300, 23);
        txtFullName.TabIndex = 1;
        //
        // lblPosition
        //
        lblPosition.AutoSize = true;
        lblPosition.Location = new Point(12, 73);
        lblPosition.Name = "lblPosition";
        lblPosition.Text = "Chức vụ:";
        //
        // txtPosition
        //
        txtPosition.Location = new Point(100, 70);
        txtPosition.MaxLength = 100;
        txtPosition.Name = "txtPosition";
        txtPosition.Size = new Size(300, 23);
        txtPosition.TabIndex = 2;
        //
        // chkIsSupervisingOfficer
        //
        chkIsSupervisingOfficer.AutoSize = true;
        chkIsSupervisingOfficer.Location = new Point(100, 101);
        chkIsSupervisingOfficer.Name = "chkIsSupervisingOfficer";
        chkIsSupervisingOfficer.TabIndex = 3;
        chkIsSupervisingOfficer.Text = "Là cán bộ quản giáo";
        //
        // chkIsActive
        //
        chkIsActive.AutoSize = true;
        chkIsActive.Location = new Point(100, 126);
        chkIsActive.Name = "chkIsActive";
        chkIsActive.TabIndex = 4;
        chkIsActive.Text = "Đang công tác";
        //
        // btnSave
        //
        btnSave.Location = new Point(244, 160);
        btnSave.Name = "btnSave";
        btnSave.Size = new Size(75, 25);
        btnSave.TabIndex = 5;
        btnSave.Text = "&Lưu";
        //
        // btnCancel
        //
        btnCancel.DialogResult = DialogResult.Cancel;
        btnCancel.Location = new Point(325, 160);
        btnCancel.Name = "btnCancel";
        btnCancel.Size = new Size(75, 25);
        btnCancel.TabIndex = 6;
        btnCancel.Text = "&Hủy";
        //
        // OfficerEditForm
        //
        AcceptButton = btnSave;
        AutoScaleMode = AutoScaleMode.Font;
        CancelButton = btnCancel;
        ClientSize = new Size(414, 197);
        Controls.Add(btnCancel);
        Controls.Add(btnSave);
        Controls.Add(chkIsActive);
        Controls.Add(chkIsSupervisingOfficer);
        Controls.Add(txtPosition);
        Controls.Add(lblPosition);
        Controls.Add(txtFullName);
        Controls.Add(lblFullName);
        Controls.Add(txtOfficerCode);
        Controls.Add(lblOfficerCode);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "OfficerEditForm";
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private Label lblOfficerCode;
    private TextBox txtOfficerCode;
    private Label lblFullName;
    private TextBox txtFullName;
    private Label lblPosition;
    private TextBox txtPosition;
    private CheckBox chkIsSupervisingOfficer;
    private CheckBox chkIsActive;
    private Button btnSave;
    private Button btnCancel;
}
