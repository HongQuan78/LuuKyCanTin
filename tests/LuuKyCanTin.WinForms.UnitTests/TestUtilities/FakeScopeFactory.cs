using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace LuuKyCanTin.WinForms.UnitTests.TestUtilities;

/// <summary>Fakes the DI scope a presenter opens for one operation, so tests need no container.</summary>
internal static class FakeScopeFactory
{
    public static IServiceScopeFactory Create(params object[] services)
    {
        var scopeFactory = Substitute.For<IServiceScopeFactory>();
        var scope = Substitute.For<IServiceScope>();
        var provider = Substitute.For<IServiceProvider>();

        scopeFactory.CreateScope().Returns(scope);
        scope.ServiceProvider.Returns(provider);

        foreach (var service in services)
            Register(provider, service);

        return scopeFactory;
    }

    private static void Register(IServiceProvider provider, object services)
    {
        var type = services.GetType();
        provider.GetService(type).Returns(services);

        // An NSubstitute proxy registers under the interfaces it fakes, not its generated class.
        foreach (var contract in type.GetInterfaces())
            provider.GetService(contract).Returns(services);
    }
}
