using System.Reflection;
using Shouldly;

namespace LuuKyCanTin.Domain.UnitTests;

public class DomainAssemblyTests
{
    [Fact]
    public void Load_DomainAssembly_Succeeds()
    {
        Assembly.Load("LuuKyCanTin.Domain").GetName().Name.ShouldBe("LuuKyCanTin.Domain");
    }
}
