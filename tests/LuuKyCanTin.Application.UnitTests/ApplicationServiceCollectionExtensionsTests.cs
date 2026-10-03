using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace LuuKyCanTin.Application.UnitTests;

public class ApplicationServiceCollectionExtensionsTests
{
    [Fact]
    public void AddApplication_AnyCollection_ReturnsSameInstance()
    {
        var services = new ServiceCollection();

        services.AddApplication().ShouldBeSameAs(services);
    }
}
