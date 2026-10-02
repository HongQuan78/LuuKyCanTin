namespace SP03Search.Data;

internal static class VietnameseNames
{
    private static readonly string[] Surnames =
    {
        "Nguyễn", "Trần", "Lê", "Phạm", "Hoàng", "Huỳnh", "Phan", "Vũ",
        "Võ", "Đặng", "Bùi", "Đỗ", "Hồ", "Ngô", "Dương", "Lý",
    };

    private static readonly string[] MiddleNames =
    {
        "Văn", "Thị", "Đức", "Minh", "Ngọc", "Hữu", "Quang", "Thanh",
        "Công", "Đình", "Xuân", "Thu", "Hồng", "Bá", "Duy", "Kim",
    };

    private static readonly string[] GivenNames =
    {
        "An", "Anh", "Bình", "Châu", "Cường", "Dũng", "Đạt", "Đông", "Giang", "Hà",
        "Hải", "Hằng", "Hiếu", "Hoa", "Hùng", "Hương", "Khánh", "Kiên", "Lan", "Linh",
        "Long", "Mai", "Nam", "Nga", "Ngân", "Nhung", "Oanh", "Phúc", "Phương", "Quân",
        "Quỳnh", "Sơn", "Tâm", "Thảo", "Thắng", "Thủy", "Tiến", "Trang", "Trung", "Tuấn",
        "Tuyết", "Vân", "Việt", "Yến",
    };

    private static readonly HashSet<string> ReservedNames = new(StringComparer.Ordinal)
    {
        "Nguyễn Văn A",
        "Đỗ Đức Đạt",
        "Trần Thị Tuyết",
        "Phan Văn Kiên",
    };

    /// <summary>
    /// Deterministic fixture (fixed seed). Rows 1-5 are namesakes "Nguyễn Văn A", rows 6-10
    /// are namesakes "Đỗ Đức Đạt", row 11 is "Trần Thị Tuyết" and row 123 is "Phan Văn Kiên".
    /// No random row may collide with those exact names, so the counts in the report are stable.
    /// </summary>
    public static List<DoiTuongSpike> Generate(int count)
    {
        var random = new Random(20261002);
        var rows = new List<DoiTuongSpike>(count);
        for (var i = 0; i < count; i++)
        {
            rows.Add(Create(i, NextName(random), random));
        }

        Assign(rows, 0, "Nguyễn Văn A");
        Assign(rows, 1, "Nguyễn Văn A");
        Assign(rows, 2, "Nguyễn Văn A");
        Assign(rows, 3, "Nguyễn Văn A");
        Assign(rows, 4, "Nguyễn Văn A");
        Assign(rows, 5, "Đỗ Đức Đạt");
        Assign(rows, 6, "Đỗ Đức Đạt");
        Assign(rows, 7, "Đỗ Đức Đạt");
        Assign(rows, 8, "Đỗ Đức Đạt");
        Assign(rows, 9, "Đỗ Đức Đạt");
        Assign(rows, 10, "Trần Thị Tuyết");
        Assign(rows, 122, "Phan Văn Kiên");

        return rows;
    }

    private static string NextName(Random random)
    {
        while (true)
        {
            var name =
                $"{Surnames[random.Next(Surnames.Length)]} " +
                $"{MiddleNames[random.Next(MiddleNames.Length)]} " +
                $"{GivenNames[random.Next(GivenNames.Length)]}";
            if (!ReservedNames.Contains(name))
            {
                return name;
            }
        }
    }

    private static DoiTuongSpike Create(int index, string name, Random random) => new()
    {
        MaSo = $"DT{index + 1:D6}",
        HoTen = name,
        HoTenKhongDau = VietnameseText.RemoveDiacritics(name),
        NamSinh = (short)(1955 + random.Next(0, 50)),
        BuongGiam = $"Buồng {1 + random.Next(0, 12)}",
    };

    private static void Assign(List<DoiTuongSpike> rows, int index, string name)
    {
        if (index >= rows.Count)
        {
            return;
        }

        rows[index].HoTen = name;
        rows[index].HoTenKhongDau = VietnameseText.RemoveDiacritics(name);
    }
}
