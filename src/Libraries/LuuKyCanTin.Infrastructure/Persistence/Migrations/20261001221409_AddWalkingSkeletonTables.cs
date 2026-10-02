using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LuuKyCanTin.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddWalkingSkeletonTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DemSoChungTu",
                columns: table => new
                {
                    LoaiChungTu = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    Nam = table.Column<short>(type: "smallint", nullable: false),
                    TienTo = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    SoHienTai = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DemSoChungTu", x => new { x.LoaiChungTu, x.Nam });
                });

            migrationBuilder.CreateTable(
                name: "DoiTuong",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaSo = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    HoTen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NamSinh = table.Column<short>(type: "smallint", nullable: true),
                    LoaiDoiTuong = table.Column<byte>(type: "tinyint", nullable: false),
                    NgayVao = table.Column<DateOnly>(type: "date", nullable: false),
                    BuongGiam = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TrangThai = table.Column<byte>(type: "tinyint", nullable: false, defaultValue: (byte)1),
                    NgayRa = table.Column<DateOnly>(type: "date", nullable: true),
                    SoDuLuuKy = table.Column<decimal>(type: "decimal(18,0)", precision: 18, scale: 0, nullable: false, defaultValue: 0m),
                    NgayTao = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false),
                    NguoiTaoId = table.Column<int>(type: "int", nullable: false),
                    NgaySua = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: true),
                    NguoiSuaId = table.Column<int>(type: "int", nullable: true),
                    RowVer = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoiTuong", x => x.Id);
                    table.CheckConstraint("CK_DoiTuong_LoaiDoiTuong", "[LoaiDoiTuong] IN (1, 2)");
                    table.CheckConstraint("CK_DoiTuong_NgayRa", "[TrangThai] = 1 OR [NgayRa] IS NOT NULL");
                    table.CheckConstraint("CK_DoiTuong_SoDuLuuKy", "[SoDuLuuKy] >= 0");
                    table.CheckConstraint("CK_DoiTuong_TrangThai", "[TrangThai] IN (1, 2, 3)");
                });

            migrationBuilder.CreateTable(
                name: "NguoiDung",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenDangNhap = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MatKhauHash = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: false),
                    DangHoatDong = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    SoLanSai = table.Column<byte>(type: "tinyint", nullable: false, defaultValue: (byte)0),
                    KhoaDen = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: true),
                    NgayTao = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false),
                    NguoiTaoId = table.Column<int>(type: "int", nullable: false),
                    NgaySua = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: true),
                    NguoiSuaId = table.Column<int>(type: "int", nullable: true),
                    RowVer = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NguoiDung", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ThongTinDonVi",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    TenCoQuanChuQuan = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TenDonVi = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DiaChi = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false),
                    NguoiTaoId = table.Column<int>(type: "int", nullable: false),
                    NgaySua = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: true),
                    NguoiSuaId = table.Column<int>(type: "int", nullable: true),
                    RowVer = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThongTinDonVi", x => x.Id);
                    table.CheckConstraint("CK_ThongTinDonVi_Id", "[Id] = 1");
                });

            migrationBuilder.CreateTable(
                name: "ChungTuLuuKy",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SoChungTu = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    NgayChungTu = table.Column<DateOnly>(type: "date", nullable: false),
                    LoaiPhieu = table.Column<byte>(type: "tinyint", nullable: false),
                    NghiepVu = table.Column<byte>(type: "tinyint", nullable: false),
                    HinhThuc = table.Column<byte>(type: "tinyint", nullable: false),
                    TrangThai = table.Column<byte>(type: "tinyint", nullable: false),
                    DoiTuongId = table.Column<int>(type: "int", nullable: false),
                    HoTenDoiTuong = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LoaiDoiTuong = table.Column<byte>(type: "tinyint", nullable: false),
                    NguoiGuiHoTen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    QuanHe = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SoPhieuGoc = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: true),
                    SoTaiKhoanNguoiGui = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: true),
                    NgayNhan = table.Column<DateOnly>(type: "date", nullable: true),
                    NoiDung = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SoTien = table.Column<decimal>(type: "decimal(18,0)", precision: 18, scale: 0, nullable: false),
                    SoTienBangChu = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    SoDuTruoc = table.Column<decimal>(type: "decimal(18,0)", precision: 18, scale: 0, nullable: false),
                    SoDuSau = table.Column<decimal>(type: "decimal(18,0)", precision: 18, scale: 0, nullable: false),
                    LyDoHuy = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    NgayHuy = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: true),
                    NguoiHuyId = table.Column<int>(type: "int", nullable: true),
                    SoLanIn = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    NgayTao = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false),
                    NguoiTaoId = table.Column<int>(type: "int", nullable: false),
                    NgaySua = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: true),
                    NguoiSuaId = table.Column<int>(type: "int", nullable: true),
                    RowVer = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChungTuLuuKy", x => x.Id);
                    table.CheckConstraint("CK_ChungTuLuuKy_ChuyenKhoan", "[HinhThuc] <> 2 OR [SoTaiKhoanNguoiGui] IS NOT NULL");
                    table.CheckConstraint("CK_ChungTuLuuKy_HinhThuc", "[HinhThuc] IN (1, 2)");
                    table.CheckConstraint("CK_ChungTuLuuKy_Huy", "[TrangThai] <> 3 OR ([LyDoHuy] IS NOT NULL AND [NgayHuy] IS NOT NULL AND [NguoiHuyId] IS NOT NULL)");
                    table.CheckConstraint("CK_ChungTuLuuKy_LoaiDoiTuong", "[LoaiDoiTuong] IN (1, 2)");
                    table.CheckConstraint("CK_ChungTuLuuKy_LoaiPhieu", "[LoaiPhieu] IN (1, 2)");
                    table.CheckConstraint("CK_ChungTuLuuKy_NghiepVu", "[NghiepVu] IN (11, 12, 13, 14, 21, 22, 23, 24, 25)");
                    table.CheckConstraint("CK_ChungTuLuuKy_NghiepVu_LoaiPhieu", "[NghiepVu] / 10 = [LoaiPhieu]");
                    table.CheckConstraint("CK_ChungTuLuuKy_SoTien", "[SoTien] > 0");
                    table.CheckConstraint("CK_ChungTuLuuKy_TrangThai", "[TrangThai] IN (1, 2, 3)");
                    table.ForeignKey(
                        name: "FK_ChungTuLuuKy_DoiTuong_DoiTuongId",
                        column: x => x.DoiTuongId,
                        principalTable: "DoiTuong",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "DemSoChungTu",
                columns: new[] { "LoaiChungTu", "Nam", "TienTo" },
                values: new object[] { "BNT", (short)2026, "BNT" });

            migrationBuilder.InsertData(
                table: "ThongTinDonVi",
                columns: new[] { "Id", "DiaChi", "NgaySua", "NgayTao", "NguoiSuaId", "NguoiTaoId", "TenCoQuanChuQuan", "TenDonVi" },
                values: new object[] { 1, "", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 0, null, "" });

            // The initial administrator, generated offline once with Pbkdf2MatKhauHasher; never compute it here,
            // because migrations must be deterministic. The initial password is in docs/install.md; Epic 2 (FR1)
            // forces a change at first sign-in.
            migrationBuilder.InsertData(
                table: "NguoiDung",
                columns: new[] { "TenDangNhap", "MatKhauHash", "DangHoatDong", "SoLanSai", "NgayTao", "NguoiTaoId" },
                values: new object[]
                {
                    "admin",
                    "PBKDF2-SHA256$600000$NMcleNM8QhZ39R2vEvPvmQ==$LJOZ1QeRys56JP1n17+ASkDFYxuLQNai0L41+HrBZsY=",
                    true,
                    (byte)0,
                    new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                    0,
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChungTuLuuKy_DoiTuongId_NgayChungTu_Id",
                table: "ChungTuLuuKy",
                columns: new[] { "DoiTuongId", "NgayChungTu", "Id" })
                .Annotation("SqlServer:Include", new[] { "LoaiPhieu", "SoTien", "TrangThai" });

            migrationBuilder.CreateIndex(
                name: "IX_ChungTuLuuKy_NgayChungTu",
                table: "ChungTuLuuKy",
                column: "NgayChungTu",
                filter: "[TrangThai] = 2");

            migrationBuilder.CreateIndex(
                name: "IX_ChungTuLuuKy_SoChungTu",
                table: "ChungTuLuuKy",
                column: "SoChungTu",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DoiTuong_HoTen",
                table: "DoiTuong",
                column: "HoTen");

            migrationBuilder.CreateIndex(
                name: "IX_DoiTuong_MaSo",
                table: "DoiTuong",
                column: "MaSo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NguoiDung_TenDangNhap",
                table: "NguoiDung",
                column: "TenDangNhap",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "NguoiDung",
                keyColumn: "TenDangNhap",
                keyValue: "admin");

            migrationBuilder.DropTable(
                name: "ChungTuLuuKy");

            migrationBuilder.DropTable(
                name: "DemSoChungTu");

            migrationBuilder.DropTable(
                name: "NguoiDung");

            migrationBuilder.DropTable(
                name: "ThongTinDonVi");

            migrationBuilder.DropTable(
                name: "DoiTuong");
        }
    }
}
