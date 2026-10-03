using LuuKyCanTin.Domain.HeThong;
using Shouldly;

namespace LuuKyCanTin.Domain.UnitTests.HeThong;

public class MatKhauTamTests
{
    // The look-alike characters a temporary password read from a screen must never contain.
    private static readonly char[] KyTuDeNham = ['0', 'O', 'o', '1', 'l', 'I'];

    [Fact]
    public void Tao_HasTheFixedLength()
    {
        MatKhauTam.Tao().Length.ShouldBe(MatKhauTam.DoDai);
    }

    [Fact]
    public void Tao_AlwaysPassesThePasswordPolicy()
    {
        for (var lan = 0; lan < 1_000; lan++)
            ChinhSachMatKhau.KiemTra(MatKhauTam.Tao()).ShouldBeEmpty();
    }

    [Fact]
    public void Tao_ContainsNoLookAlikeCharacter()
    {
        for (var lan = 0; lan < 1_000; lan++)
            MatKhauTam.Tao().ShouldAllBe(kyTu => !KyTuDeNham.Contains(kyTu));
    }

    [Fact]
    public void Tao_TwoCallsDiffer()
    {
        MatKhauTam.Tao().ShouldNotBe(MatKhauTam.Tao());
    }
}
