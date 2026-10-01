using System.Reflection;
using Shouldly;

namespace LuuKyCanTin.Domain.UnitTests;

public class DomainAssemblyTests
{
    [Fact]
    public void DomainAssembly_IsLoadable()
    {
        Assembly.Load("LuuKyCanTin.Domain").GetName().Name.ShouldBe("LuuKyCanTin.Domain");
    }
}
