using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace LuuKyCanTin.Application.UnitTests;

public class DependencyInjectionTests
{
    [Fact]
    public void AddApplication_ReturnsSameCollectionForChaining()
    {
        var services = new ServiceCollection();

        services.AddApplication().ShouldBeSameAs(services);
    }
}
