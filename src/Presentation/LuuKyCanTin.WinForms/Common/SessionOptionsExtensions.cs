using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LuuKyCanTin.WinForms.Common;

/// <summary>Registers the session settings with the environment-aware idle-lock validation.</summary>
internal static class SessionOptionsExtensions
{
    public static IServiceCollection AddSessionOptions(
        this IServiceCollection services, IConfiguration configuration, bool isDevelopment)
    {
        services.AddOptions<SessionOptions>()
            .Bind(configuration.GetSection(SessionOptions.SectionName))
            .Validate(
                options => SessionOptions.IsValidIdleLockMinutes(options.IdleLockMinutes, isDevelopment),
                "Session:IdleLockMinutes must be between 1 and 60, or 0 in Development to turn auto-lock off.");
        return services;
    }
}
