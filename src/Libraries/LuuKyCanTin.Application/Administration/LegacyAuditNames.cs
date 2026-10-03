namespace LuuKyCanTin.Application.Administration;

/// <summary>
/// The names audit-log rows written before the English rename (migration <c>RenameIdentifiersToEnglish</c>) still hold.
/// The log is append-only, so those rows keep their Vietnamese table names, JSON property names and enum value names;
/// the audit-log viewer translates them through these maps to show old and new rows alike.
/// </summary>
public static class LegacyAuditNames
{
    /// <summary>Old table name → current table name, as stored in <c>AuditLog.TableName</c>.</summary>
    public static readonly IReadOnlyDictionary<string, string> Tables = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["CanBo"] = "Officer",
        ["DoiTuong"] = "Inmate",
        ["ChungTuLuuKy"] = "CustodyVoucher",
        ["DemSoChungTu"] = "VoucherCounter",
        ["NguoiDung"] = "User",
        ["NguoiDungVaiTro"] = "UserRole",
        ["NhatKyThaoTac"] = "AuditLog",
        ["Quyen"] = "Permission",
        ["ThongTinDonVi"] = "FacilityInfo",
        ["VaiTro"] = "Role",
        ["VaiTroQuyen"] = "RolePermission",
    };

    /// <summary>
    /// Per current table name: old column or payload property name → current name, as stored in the keys of
    /// <c>AuditLog.OldValues</c> and <c>AuditLog.NewValues</c>.
    /// </summary>
    public static readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> Properties =
        new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.Ordinal)
        {
            ["Officer"] = WithAuditColumns(new()
            {
                ["MaCanBo"] = "OfficerCode",
                ["HoTen"] = "FullName",
                ["ChucVu"] = "Position",
                ["LaQuanGiao"] = "IsSupervisingOfficer",
                ["DangCongTac"] = "IsActive",
            }),
            ["Inmate"] = WithAuditColumns(new()
            {
                ["MaSo"] = "InmateCode",
                ["HoTen"] = "FullName",
                ["NamSinh"] = "BirthYear",
                ["LoaiDoiTuong"] = "InmateType",
                ["NgayVao"] = "AdmissionDate",
                ["BuongGiam"] = "Cell",
                ["TrangThai"] = "Status",
                ["NgayRa"] = "ReleaseDate",
                ["SoDuLuuKy"] = "CustodyBalance",
            }),
            ["CustodyVoucher"] = WithAuditColumns(new()
            {
                ["SoChungTu"] = "VoucherNumber",
                ["NgayChungTu"] = "VoucherDate",
                ["LoaiPhieu"] = "VoucherType",
                ["NghiepVu"] = "TransactionType",
                ["HinhThuc"] = "PaymentMethod",
                ["TrangThai"] = "Status",
                ["DoiTuongId"] = "InmateId",
                ["HoTenDoiTuong"] = "InmateFullName",
                ["LoaiDoiTuong"] = "InmateType",
                ["NguoiGuiHoTen"] = "SenderFullName",
                ["QuanHe"] = "Relationship",
                ["SoPhieuGoc"] = "SourceDocumentNumber",
                ["SoTaiKhoanNguoiGui"] = "SenderAccountNumber",
                ["NgayNhan"] = "ReceivedDate",
                ["NoiDung"] = "Description",
                ["SoTien"] = "Amount",
                ["SoTienBangChu"] = "AmountInWords",
                ["SoDuTruoc"] = "BalanceBefore",
                ["SoDuSau"] = "BalanceAfter",
                ["LyDoHuy"] = "CancellationReason",
                ["NgayHuy"] = "CancelledAt",
                ["NguoiHuyId"] = "CancelledById",
                ["SoLanIn"] = "PrintCount",
            }),
            ["VoucherCounter"] = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["LoaiChungTu"] = "VoucherTypeCode",
                ["Nam"] = "Year",
                ["TienTo"] = "Prefix",
                ["SoHienTai"] = "CurrentNumber",
            },
            // Sign-in events store their payload under the account's table.
            ["User"] = WithAuditColumns(new()
            {
                ["TenDangNhap"] = "UserName",
                ["MatKhauHash"] = "PasswordHash",
                ["DangHoatDong"] = "IsActive",
                ["PhaiDoiMatKhau"] = "MustChangePassword",
                ["SoLanSai"] = "FailedAttemptCount",
                ["KhoaDen"] = "LockedUntil",
                ["SuKien"] = "Event",
            }),
            ["UserRole"] = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["NguoiDungId"] = "UserId",
                ["VaiTroId"] = "RoleId",
            },
            ["Permission"] = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["Ma"] = "Code",
                ["Ten"] = "Name",
            },
            ["FacilityInfo"] = WithAuditColumns(new()
            {
                ["TenCoQuanChuQuan"] = "ParentAgencyName",
                ["TenDonVi"] = "FacilityName",
                ["DiaChi"] = "Address",
            }),
            // Permission edits store the before/after code lists under "Quyen".
            ["Role"] = WithAuditColumns(new()
            {
                ["Ma"] = "Code",
                ["Ten"] = "Name",
                ["Quyen"] = "Permissions",
            }),
            ["RolePermission"] = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["VaiTroId"] = "RoleId",
                ["QuyenId"] = "PermissionId",
            },
        };

    /// <summary>
    /// Old enum value name → current name, as the audit-log JSON wrote enum values by name. The audit action column
    /// itself keeps its stored codes and needs no translation.
    /// </summary>
    public static readonly IReadOnlyDictionary<string, string> EnumValues = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        // VoucherStatus
        ["Nhap"] = "Draft",
        ["DaGhiSo"] = "Posted",
        ["DaHuy"] = "Cancelled",
        // VoucherType
        ["Thu"] = "Receipt",
        ["Chi"] = "Payout",
        // PaymentMethod
        ["TienMat"] = "Cash",
        ["ChuyenKhoan"] = "BankTransfer",
        // InmateType
        ["TamGiuTamGiam"] = "PreTrialDetainee",
        ["PhamNhan"] = "Prisoner",
        // InmateStatus
        ["DangQuanLy"] = "InCustody",
        ["DaChuyenTrai"] = "FacilityTransferred",
        ["DaChapHanhXongAn"] = "SentenceCompleted",
        // TransactionType
        ["MangTheoKhiVao"] = "BroughtOnAdmission",
        ["NguoiThanGui"] = "SentByRelative",
        ["PhieuGuiQua"] = "GiftSlip",
        ["NhanTuDoiTuongKhac"] = "ReceivedFromOtherInmate",
        ["MuaHang"] = "CanteenPurchase",
        ["ChoTien"] = "GivenToOtherInmate",
        ["ChuyenVeNguoiThan"] = "ReturnedToRelative",
        ["ChuyenTrai"] = "FacilityTransfer",
        ["ChapHanhXongAn"] = "SentenceCompleted",
    };

    private static Dictionary<string, string> WithAuditColumns(Dictionary<string, string> columns)
    {
        var result = new Dictionary<string, string>(columns, StringComparer.Ordinal)
        {
            ["NgayTao"] = "CreatedAt",
            ["NguoiTaoId"] = "CreatedById",
            ["NgaySua"] = "ModifiedAt",
            ["NguoiSuaId"] = "ModifiedById",
        };
        return result;
    }
}
