-- SP-03 throw-away schema (Story 1.7). Mirrors the product's Story 1.2 setup:
-- database-level collation Vietnamese_CI_AI, so names can be searched without diacritics.
-- Run this script only against a local/dev server. It DROPS the database if it exists.

IF DB_ID(N'LuuKyCanTin_Spike_Search') IS NOT NULL
BEGIN
    ALTER DATABASE [LuuKyCanTin_Spike_Search] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [LuuKyCanTin_Spike_Search];
END
GO

CREATE DATABASE [LuuKyCanTin_Spike_Search] COLLATE Vietnamese_CI_AI;
GO

ALTER DATABASE [LuuKyCanTin_Spike_Search] SET RECOVERY SIMPLE;
GO

USE [LuuKyCanTin_Spike_Search];
GO

-- Mirrors the DB design's DoiTuong columns used by the picker (UX-DR2).
-- HoTenKhongDau is the spike's candidate for option (a): a persisted diacritic-stripped
-- column (đ->d) filled by the Application layer on save, indexed for prefix seeks.
CREATE TABLE dbo.DoiTuongSpike
(
    Id            int IDENTITY(1,1) NOT NULL CONSTRAINT PK_DoiTuongSpike PRIMARY KEY,
    MaSo          varchar(30)   NOT NULL,
    HoTen         nvarchar(100) NOT NULL,
    HoTenKhongDau nvarchar(100) NULL,
    NamSinh       smallint      NOT NULL,
    BuongGiam     nvarchar(50)  NULL,
    CONSTRAINT UQ_DoiTuongSpike_MaSo UNIQUE (MaSo)
);
GO

CREATE INDEX IX_HoTen ON dbo.DoiTuongSpike (HoTen);
GO

CREATE INDEX IX_HoTenKhongDau ON dbo.DoiTuongSpike (HoTenKhongDau);
GO
