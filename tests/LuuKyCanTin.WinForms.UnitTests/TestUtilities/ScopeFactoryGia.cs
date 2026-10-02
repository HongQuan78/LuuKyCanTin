using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace LuuKyCanTin.WinForms.UnitTests.TestUtilities;

/// <summary>Fakes the DI scope a presenter opens for one operation, so tests need no container.</summary>
internal static class ScopeFactoryGia
{
    public static IServiceScopeFactory Tao(params object[] dichVu)
    {
        var scopeFactory = Substitute.For<IServiceScopeFactory>();
        var scope = Substitute.For<IServiceScope>();
        var provider = Substitute.For<IServiceProvider>();

        scopeFactory.CreateScope().Returns(scope);
        scope.ServiceProvider.Returns(provider);

        foreach (var dichVu1 in dichVu)
            DangKy(provider, dichVu1);

        return scopeFactory;
    }

    private static void DangKy(IServiceProvider provider, object dichVu)
    {
        var type = dichVu.GetType();
        provider.GetService(type).Returns(dichVu);

        // An NSubstitute proxy registers under the interfaces it fakes, not its generated class.
        foreach (var giaoDien in type.GetInterfaces())
            provider.GetService(giaoDien).Returns(dichVu);
    }
}
