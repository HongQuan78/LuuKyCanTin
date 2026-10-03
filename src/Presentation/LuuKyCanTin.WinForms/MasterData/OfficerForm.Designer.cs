namespace LuuKyCanTin.WinForms.MasterData;

partial class OfficerForm
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
        lblKeyword = new Label();
        txtKeyword = new TextBox();
        chkShowInactive = new CheckBox();
        btnAdd = new Button();
        btnEdit = new Button();
        grdOfficers = new DataGridView();
        colOfficerCode = new DataGridViewTextBoxColumn();
        colFullName = new DataGridViewTextBoxColumn();
        colPosition = new DataGridViewTextBoxColumn();
        colIsSupervisingOfficer = new DataGridViewCheckBoxColumn();
        colIsActive = new DataGridViewCheckBoxColumn();
        tmrSearch = new System.Windows.Forms.Timer(components);
        ((System.ComponentModel.ISupportInitialize)grdOfficers).BeginInit();
        SuspendLayout();
        //
        // lblKeyword
        //
        lblKeyword.AutoSize = true;
        lblKeyword.Location = new Point(12, 15);
        lblKeyword.Name = "lblKeyword";
        lblKeyword.Text = "Tìm (mã hoặc tên):";
        //
        // txtKeyword
        //
        txtKeyword.Location = new Point(130, 12);
        txtKeyword.Name = "txtKeyword";
        txtKeyword.Size = new Size(260, 23);
        txtKeyword.TabIndex = 0;
        //
        // chkShowInactive
        //
        chkShowInactive.AutoSize = true;
        chkShowInactive.Location = new Point(405, 14);
        chkShowInactive.Name = "chkShowInactive";
        chkShowInactive.TabIndex = 1;
        chkShowInactive.Text = "Hiện cả người đã nghỉ";
        //
        // btnAdd
        //
        btnAdd.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnAdd.Location = new Point(616, 11);
        btnAdd.Name = "btnAdd";
        btnAdd.Size = new Size(75, 25);
        btnAdd.TabIndex = 2;
        btnAdd.Text = "&Thêm";
        //
        // btnEdit
        //
        btnEdit.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnEdit.Location = new Point(697, 11);
        btnEdit.Name = "btnEdit";
        btnEdit.Size = new Size(75, 25);
        btnEdit.TabIndex = 3;
        btnEdit.Text = "&Sửa";
        //
        // grdOfficers
        //
        grdOfficers.AllowUserToAddRows = false;
        grdOfficers.AllowUserToDeleteRows = false;
        grdOfficers.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        grdOfficers.AutoGenerateColumns = false;
        grdOfficers.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grdOfficers.Columns.AddRange(new DataGridViewColumn[] { colOfficerCode, colFullName, colPosition, colIsSupervisingOfficer, colIsActive });
        grdOfficers.Location = new Point(12, 45);
        grdOfficers.MultiSelect = false;
        grdOfficers.Name = "grdOfficers";
        grdOfficers.ReadOnly = true;
        grdOfficers.RowHeadersVisible = false;
        grdOfficers.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grdOfficers.Size = new Size(760, 404);
        grdOfficers.TabIndex = 4;
        //
        // colOfficerCode
        //
        colOfficerCode.FillWeight = 15F;
        colOfficerCode.HeaderText = "Mã";
        colOfficerCode.Name = "colOfficerCode";
        //
        // colFullName
        //
        colFullName.FillWeight = 35F;
        colFullName.HeaderText = "Họ tên";
        colFullName.Name = "colFullName";
        //
        // colPosition
        //
        colPosition.FillWeight = 30F;
        colPosition.HeaderText = "Chức vụ";
        colPosition.Name = "colPosition";
        //
        // colIsSupervisingOfficer
        //
        colIsSupervisingOfficer.FillWeight = 10F;
        colIsSupervisingOfficer.HeaderText = "Quản giáo";
        colIsSupervisingOfficer.Name = "colIsSupervisingOfficer";
        //
        // colIsActive
        //
        colIsActive.FillWeight = 10F;
        colIsActive.HeaderText = "Đang công tác";
        colIsActive.Name = "colIsActive";
        //
        // tmrSearch
        //
        tmrSearch.Interval = 300;
        //
        // OfficerForm
        //
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(784, 461);
        Controls.Add(grdOfficers);
        Controls.Add(btnEdit);
        Controls.Add(btnAdd);
        Controls.Add(chkShowInactive);
        Controls.Add(txtKeyword);
        Controls.Add(lblKeyword);
        MinimumSize = new Size(640, 360);
        Name = "OfficerForm";
        StartPosition = FormStartPosition.CenterParent;
        Text = "Danh mục cán bộ";
        ((System.ComponentModel.ISupportInitialize)grdOfficers).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private Label lblKeyword;
    private TextBox txtKeyword;
    private CheckBox chkShowInactive;
    private Button btnAdd;
    private Button btnEdit;
    private DataGridView grdOfficers;
    private DataGridViewTextBoxColumn colOfficerCode;
    private DataGridViewTextBoxColumn colFullName;
    private DataGridViewTextBoxColumn colPosition;
    private DataGridViewCheckBoxColumn colIsSupervisingOfficer;
    private DataGridViewCheckBoxColumn colIsActive;
    private System.Windows.Forms.Timer tmrSearch;
}
