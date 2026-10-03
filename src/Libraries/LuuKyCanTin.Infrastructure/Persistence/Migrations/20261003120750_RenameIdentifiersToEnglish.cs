using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LuuKyCanTin.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Renames every table, column, key and index to English in place, so every row survives. Written by hand: the
    /// scaffolded version dropped and recreated every table. CHECK constraints and the filtered index are dropped and
    /// recreated because their SQL text names columns; the rows they check are unchanged. The audit log's stored table
    /// names, JSON property names and action codes are data and are left as they were.
    /// </summary>
    public partial class RenameIdentifiersToEnglish : Migration
    {
        private static readonly (string Old, string New)[] Tables =
        [
            ("CanBo", "Officer"),
            ("DoiTuong", "Inmate"),
            ("ChungTuLuuKy", "CustodyVoucher"),
            ("DemSoChungTu", "VoucherCounter"),
            ("NguoiDung", "User"),
            ("NguoiDungVaiTro", "UserRole"),
            ("NhatKyThaoTac", "AuditLog"),
            ("Quyen", "Permission"),
            ("ThongTinDonVi", "FacilityInfo"),
            ("VaiTro", "Role"),
            ("VaiTroQuyen", "RolePermission"),
        ];

        private static readonly (string Old, string New)[] AuditColumns =
        [
            ("NgayTao", "CreatedAt"),
            ("NguoiTaoId", "CreatedById"),
            ("NgaySua", "ModifiedAt"),
            ("NguoiSuaId", "ModifiedById"),
        ];

        // Keyed by the new table name: the tables are renamed first.
        private static readonly (string Table, string Old, string New)[] Columns =
        [
            ("Officer", "MaCanBo", "OfficerCode"),
            ("Officer", "HoTen", "FullName"),
            ("Officer", "ChucVu", "Position"),
            ("Officer", "LaQuanGiao", "IsSupervisingOfficer"),
            ("Officer", "DangCongTac", "IsActive"),
            ("Inmate", "MaSo", "InmateCode"),
            ("Inmate", "HoTen", "FullName"),
            ("Inmate", "NamSinh", "BirthYear"),
            ("Inmate", "LoaiDoiTuong", "InmateType"),
            ("Inmate", "NgayVao", "AdmissionDate"),
            ("Inmate", "BuongGiam", "Cell"),
            ("Inmate", "TrangThai", "Status"),
            ("Inmate", "NgayRa", "ReleaseDate"),
            ("Inmate", "SoDuLuuKy", "CustodyBalance"),
            ("CustodyVoucher", "SoChungTu", "VoucherNumber"),
            ("CustodyVoucher", "NgayChungTu", "VoucherDate"),
            ("CustodyVoucher", "LoaiPhieu", "VoucherType"),
            ("CustodyVoucher", "NghiepVu", "TransactionType"),
            ("CustodyVoucher", "HinhThuc", "PaymentMethod"),
            ("CustodyVoucher", "TrangThai", "Status"),
            ("CustodyVoucher", "DoiTuongId", "InmateId"),
            ("CustodyVoucher", "HoTenDoiTuong", "InmateFullName"),
            ("CustodyVoucher", "LoaiDoiTuong", "InmateType"),
            ("CustodyVoucher", "NguoiGuiHoTen", "SenderFullName"),
            ("CustodyVoucher", "QuanHe", "Relationship"),
            ("CustodyVoucher", "SoPhieuGoc", "SourceDocumentNumber"),
            ("CustodyVoucher", "SoTaiKhoanNguoiGui", "SenderAccountNumber"),
            ("CustodyVoucher", "NgayNhan", "ReceivedDate"),
            ("CustodyVoucher", "NoiDung", "Description"),
            ("CustodyVoucher", "SoTien", "Amount"),
            ("CustodyVoucher", "SoTienBangChu", "AmountInWords"),
            ("CustodyVoucher", "SoDuTruoc", "BalanceBefore"),
            ("CustodyVoucher", "SoDuSau", "BalanceAfter"),
            ("CustodyVoucher", "LyDoHuy", "CancellationReason"),
            ("CustodyVoucher", "NgayHuy", "CancelledAt"),
            ("CustodyVoucher", "NguoiHuyId", "CancelledById"),
            ("CustodyVoucher", "SoLanIn", "PrintCount"),
            ("VoucherCounter", "LoaiChungTu", "VoucherTypeCode"),
            ("VoucherCounter", "Nam", "Year"),
            ("VoucherCounter", "TienTo", "Prefix"),
            ("VoucherCounter", "SoHienTai", "CurrentNumber"),
            ("User", "TenDangNhap", "UserName"),
            ("User", "MatKhauHash", "PasswordHash"),
            ("User", "DangHoatDong", "IsActive"),
            ("User", "PhaiDoiMatKhau", "MustChangePassword"),
            ("User", "SoLanSai", "FailedAttemptCount"),
            ("User", "KhoaDen", "LockedUntil"),
            ("UserRole", "NguoiDungId", "UserId"),
            ("UserRole", "VaiTroId", "RoleId"),
            ("AuditLog", "ThoiDiem", "OccurredAt"),
            ("AuditLog", "NguoiDungId", "UserId"),
            ("AuditLog", "MayTram", "Workstation"),
            ("AuditLog", "HanhDong", "Action"),
            ("AuditLog", "TenBang", "TableName"),
            ("AuditLog", "BanGhiId", "RecordId"),
            ("AuditLog", "DuLieuCu", "OldValues"),
            ("AuditLog", "DuLieuMoi", "NewValues"),
            ("Permission", "Ma", "Code"),
            ("Permission", "Ten", "Name"),
            ("FacilityInfo", "TenCoQuanChuQuan", "ParentAgencyName"),
            ("FacilityInfo", "TenDonVi", "FacilityName"),
            ("FacilityInfo", "DiaChi", "Address"),
            ("Role", "Ma", "Code"),
            ("Role", "Ten", "Name"),
            ("RolePermission", "VaiTroId", "RoleId"),
            ("RolePermission", "QuyenId", "PermissionId"),
        ];

        private static readonly string[] TablesWithAuditColumns = ["Officer", "Inmate", "CustodyVoucher", "User", "FacilityInfo", "Role"];

        private static readonly (string Table, string Old, string New)[] Indexes =
        [
            ("Officer", "IX_CanBo_HoTen", "IX_Officer_FullName"),
            ("Officer", "IX_CanBo_MaCanBo", "IX_Officer_OfficerCode"),
            ("CustodyVoucher", "IX_ChungTuLuuKy_DoiTuongId_NgayChungTu_Id", "IX_CustodyVoucher_InmateId_VoucherDate_Id"),
            ("CustodyVoucher", "IX_ChungTuLuuKy_SoChungTu", "IX_CustodyVoucher_VoucherNumber"),
            ("Inmate", "IX_DoiTuong_HoTen", "IX_Inmate_FullName"),
            ("Inmate", "IX_DoiTuong_MaSo", "IX_Inmate_InmateCode"),
            ("User", "IX_NguoiDung_TenDangNhap", "IX_User_UserName"),
            ("UserRole", "IX_NguoiDungVaiTro_VaiTroId", "IX_UserRole_RoleId"),
            ("AuditLog", "IX_NhatKyThaoTac_TenBang_BanGhiId", "IX_AuditLog_TableName_RecordId"),
            ("AuditLog", "IX_NhatKyThaoTac_ThoiDiem", "IX_AuditLog_OccurredAt"),
            ("Permission", "IX_Quyen_Ma", "IX_Permission_Code"),
            ("Role", "IX_VaiTro_Ma", "IX_Role_Code"),
            ("RolePermission", "IX_VaiTroQuyen_QuyenId", "IX_RolePermission_PermissionId"),
        ];

        // Primary and foreign keys: RenameTable leaves their names alone.
        private static readonly (string Old, string New)[] Keys =
        [
            ("PK_CanBo", "PK_Officer"),
            ("PK_DoiTuong", "PK_Inmate"),
            ("PK_ChungTuLuuKy", "PK_CustodyVoucher"),
            ("PK_DemSoChungTu", "PK_VoucherCounter"),
            ("PK_NguoiDung", "PK_User"),
            ("PK_NguoiDungVaiTro", "PK_UserRole"),
            ("PK_NhatKyThaoTac", "PK_AuditLog"),
            ("PK_Quyen", "PK_Permission"),
            ("PK_ThongTinDonVi", "PK_FacilityInfo"),
            ("PK_VaiTro", "PK_Role"),
            ("PK_VaiTroQuyen", "PK_RolePermission"),
            ("FK_ChungTuLuuKy_DoiTuong_DoiTuongId", "FK_CustodyVoucher_Inmate_InmateId"),
            ("FK_NguoiDungVaiTro_NguoiDung_NguoiDungId", "FK_UserRole_User_UserId"),
            ("FK_NguoiDungVaiTro_VaiTro_VaiTroId", "FK_UserRole_Role_RoleId"),
            ("FK_VaiTroQuyen_Quyen_QuyenId", "FK_RolePermission_Permission_PermissionId"),
            ("FK_VaiTroQuyen_VaiTro_VaiTroId", "FK_RolePermission_Role_RoleId"),
        ];

        private sealed record Check(string OldTable, string OldName, string OldSql, string NewTable, string NewName, string NewSql);

        private static readonly Check[] Checks =
        [
            new("DoiTuong", "CK_DoiTuong_LoaiDoiTuong", "[LoaiDoiTuong] IN (1, 2)",
                "Inmate", "CK_Inmate_InmateType", "[InmateType] IN (1, 2)"),
            new("DoiTuong", "CK_DoiTuong_NgayRa", "[TrangThai] = 1 OR [NgayRa] IS NOT NULL",
                "Inmate", "CK_Inmate_ReleaseDate", "[Status] = 1 OR [ReleaseDate] IS NOT NULL"),
            new("DoiTuong", "CK_DoiTuong_SoDuLuuKy", "[SoDuLuuKy] >= 0",
                "Inmate", "CK_Inmate_CustodyBalance", "[CustodyBalance] >= 0"),
            new("DoiTuong", "CK_DoiTuong_TrangThai", "[TrangThai] IN (1, 2, 3)",
                "Inmate", "CK_Inmate_Status", "[Status] IN (1, 2, 3)"),
            new("NhatKyThaoTac", "CK_NhatKyThaoTac_HanhDong", "[HanhDong] IN ('Them', 'Sua', 'Huy', 'In', 'Duyet', 'DangNhap')",
                "AuditLog", "CK_AuditLog_Action", "[Action] IN ('Them', 'Sua', 'Huy', 'In', 'Duyet', 'DangNhap')"),
            new("ThongTinDonVi", "CK_ThongTinDonVi_Id", "[Id] = 1",
                "FacilityInfo", "CK_FacilityInfo_Id", "[Id] = 1"),
            new("ChungTuLuuKy", "CK_ChungTuLuuKy_ChuyenKhoan", "[HinhThuc] <> 2 OR [SoTaiKhoanNguoiGui] IS NOT NULL",
                "CustodyVoucher", "CK_CustodyVoucher_BankTransfer", "[PaymentMethod] <> 2 OR [SenderAccountNumber] IS NOT NULL"),
            new("ChungTuLuuKy", "CK_ChungTuLuuKy_HinhThuc", "[HinhThuc] IN (1, 2)",
                "CustodyVoucher", "CK_CustodyVoucher_PaymentMethod", "[PaymentMethod] IN (1, 2)"),
            new("ChungTuLuuKy", "CK_ChungTuLuuKy_Huy",
                "[TrangThai] <> 3 OR ([LyDoHuy] IS NOT NULL AND [NgayHuy] IS NOT NULL AND [NguoiHuyId] IS NOT NULL)",
                "CustodyVoucher", "CK_CustodyVoucher_Cancellation",
                "[Status] <> 3 OR ([CancellationReason] IS NOT NULL AND [CancelledAt] IS NOT NULL AND [CancelledById] IS NOT NULL)"),
            new("ChungTuLuuKy", "CK_ChungTuLuuKy_LoaiDoiTuong", "[LoaiDoiTuong] IN (1, 2)",
                "CustodyVoucher", "CK_CustodyVoucher_InmateType", "[InmateType] IN (1, 2)"),
            new("ChungTuLuuKy", "CK_ChungTuLuuKy_LoaiPhieu", "[LoaiPhieu] IN (1, 2)",
                "CustodyVoucher", "CK_CustodyVoucher_VoucherType", "[VoucherType] IN (1, 2)"),
            new("ChungTuLuuKy", "CK_ChungTuLuuKy_NghiepVu", "[NghiepVu] IN (11, 12, 13, 14, 21, 22, 23, 24, 25)",
                "CustodyVoucher", "CK_CustodyVoucher_TransactionType", "[TransactionType] IN (11, 12, 13, 14, 21, 22, 23, 24, 25)"),
            new("ChungTuLuuKy", "CK_ChungTuLuuKy_NghiepVu_LoaiPhieu", "[NghiepVu] / 10 = [LoaiPhieu]",
                "CustodyVoucher", "CK_CustodyVoucher_TransactionType_VoucherType", "[TransactionType] / 10 = [VoucherType]"),
            new("ChungTuLuuKy", "CK_ChungTuLuuKy_SoTien", "[SoTien] > 0",
                "CustodyVoucher", "CK_CustodyVoucher_Amount", "[Amount] > 0"),
            new("ChungTuLuuKy", "CK_ChungTuLuuKy_TrangThai", "[TrangThai] IN (1, 2, 3)",
                "CustodyVoucher", "CK_CustodyVoucher_Status", "[Status] IN (1, 2, 3)"),
        ];

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            foreach (var check in Checks)
                migrationBuilder.DropCheckConstraint(check.OldName, check.OldTable);

            // Its filter names a column, so it is recreated instead of renamed.
            migrationBuilder.DropIndex("IX_ChungTuLuuKy_NgayChungTu", "ChungTuLuuKy");

            foreach (var (oldName, newName) in Tables)
                migrationBuilder.RenameTable(oldName, newName: newName);

            foreach (var table in TablesWithAuditColumns)
            {
                foreach (var (oldName, newName) in AuditColumns)
                    migrationBuilder.RenameColumn(oldName, table, newName);
            }

            foreach (var (table, oldName, newName) in Columns)
                migrationBuilder.RenameColumn(oldName, table, newName);

            foreach (var (table, oldName, newName) in Indexes)
                migrationBuilder.RenameIndex(oldName, newName, table);

            foreach (var (oldName, newName) in Keys)
                RenameObject(migrationBuilder, oldName, newName);

            migrationBuilder.CreateIndex("IX_CustodyVoucher_VoucherDate", "CustodyVoucher", "VoucherDate", filter: "[Status] = 2");

            foreach (var check in Checks)
                migrationBuilder.AddCheckConstraint(check.NewName, check.NewTable, check.NewSql);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            foreach (var check in Checks)
                migrationBuilder.DropCheckConstraint(check.NewName, check.NewTable);

            migrationBuilder.DropIndex("IX_CustodyVoucher_VoucherDate", "CustodyVoucher");

            foreach (var (oldName, newName) in Keys)
                RenameObject(migrationBuilder, newName, oldName);

            foreach (var (table, oldName, newName) in Indexes)
                migrationBuilder.RenameIndex(newName, oldName, table);

            foreach (var (table, oldName, newName) in Columns)
                migrationBuilder.RenameColumn(newName, table, oldName);

            foreach (var table in TablesWithAuditColumns)
            {
                foreach (var (oldName, newName) in AuditColumns)
                    migrationBuilder.RenameColumn(newName, table, oldName);
            }

            foreach (var (oldName, newName) in Tables)
                migrationBuilder.RenameTable(newName, newName: oldName);

            migrationBuilder.CreateIndex("IX_ChungTuLuuKy_NgayChungTu", "ChungTuLuuKy", "NgayChungTu", filter: "[TrangThai] = 2");

            foreach (var check in Checks)
                migrationBuilder.AddCheckConstraint(check.OldName, check.OldTable, check.OldSql);
        }

        private static void RenameObject(MigrationBuilder migrationBuilder, string oldName, string newName) =>
            migrationBuilder.Sql($"EXEC sp_rename N'[dbo].[{oldName}]', N'{newName}', N'OBJECT';");
    }
}
