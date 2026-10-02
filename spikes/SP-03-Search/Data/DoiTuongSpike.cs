namespace SP03Search.Data;

internal sealed class DoiTuongSpike
{
    public int Id { get; set; }

    public string MaSo { get; set; } = string.Empty;

    public string HoTen { get; set; } = string.Empty;

    public string? HoTenKhongDau { get; set; }

    public short NamSinh { get; set; }

    public string? BuongGiam { get; set; }
}
