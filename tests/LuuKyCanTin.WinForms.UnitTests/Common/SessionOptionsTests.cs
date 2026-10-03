using LuuKyCanTin.WinForms.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shouldly;

namespace LuuKyCanTin.WinForms.UnitTests.Common;

public class SessionOptionsTests
{
    [Theory]
    [InlineData(0, false, false)]
    [InlineData(0, true, true)]
    [InlineData(1, false, true)]
    [InlineData(60, true, true)]
    [InlineData(-1, true, false)]
    [InlineData(61, true, false)]
    public void IsValidIdleLockMinutes_OnlyOneToSixtyAndZeroInDevelopment(int minutes, bool isDevelopment, bool expected)
    {
        SessionOptions.IsValidIdleLockMinutes(minutes, isDevelopment).ShouldBe(expected);
    }

    [Fact]
    public void IdleTimeout_ZeroMinutes_IsOff()
    {
        new SessionOptions { IdleLockMinutes = 0 }.IdleTimeout.ShouldBeNull();
    }

    [Fact]
    public void IdleTimeout_FiveMinutes_IsFiveMinutes()
    {
        new SessionOptions { IdleLockMinutes = 5 }.IdleTimeout.ShouldBe(TimeSpan.FromMinutes(5));
    }

    [Fact]
    public void IdleTimeout_OutOfRange_IsOffSoNoNonsenseTimerIsBuilt()
    {
        new SessionOptions { IdleLockMinutes = -5 }.IdleTimeout.ShouldBeNull();
        new SessionOptions { IdleLockMinutes = 61 }.IdleTimeout.ShouldBeNull();
    }

    [Theory]
    [InlineData(-1, false)]
    [InlineData(61, false)]
    [InlineData(0, false)]
    public void AddSessionOptions_InvalidValue_ThrowsWhenResolved(int minutes, bool isDevelopment)
    {
        using var provider = BuildProvider(minutes, isDevelopment);

        Should.Throw<OptionsValidationException>(() => _ = provider.GetRequiredService<IOptions<SessionOptions>>().Value);
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(1, false)]
    [InlineData(5, true)]
    [InlineData(60, false)]
    public void AddSessionOptions_ValidValue_Resolves(int minutes, bool isDevelopment)
    {
        using var provider = BuildProvider(minutes, isDevelopment);

        provider.GetRequiredService<IOptions<SessionOptions>>().Value.IdleLockMinutes.ShouldBe(minutes);
    }

    private static ServiceProvider BuildProvider(int minutes, bool isDevelopment)
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection([new KeyValuePair<string, string?>("Session:IdleLockMinutes", minutes.ToString())])
            .Build();
        services.AddSessionOptions(configuration, isDevelopment);
        return services.BuildServiceProvider();
    }
}
