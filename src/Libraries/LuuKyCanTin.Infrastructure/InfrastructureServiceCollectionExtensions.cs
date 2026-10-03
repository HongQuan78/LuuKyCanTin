using LuuKyCanTin.Application.Abstractions;
using LuuKyCanTin.Application.Administration;
using LuuKyCanTin.Application.Custody;
using LuuKyCanTin.Application.MasterData;
using LuuKyCanTin.Application.Reporting;
using LuuKyCanTin.Infrastructure.Administration;
using LuuKyCanTin.Infrastructure.Common;
using LuuKyCanTin.Infrastructure.Custody;
using LuuKyCanTin.Infrastructure.MasterData;
using LuuKyCanTin.Infrastructure.Persistence;
using LuuKyCanTin.Infrastructure.Persistence.Interceptors;
using LuuKyCanTin.Infrastructure.Persistence.Seed.Demo;
using LuuKyCanTin.Infrastructure.Reports;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LuuKyCanTin.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
    public const string ConnectionStringName = "LuuKyCanTin";

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(ConnectionStringName);
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException(
                $"Missing connection string 'ConnectionStrings:{ConnectionStringName}'. Set ConnectionStrings__{ConnectionStringName} "
                + "in the .env file next to the exe (see .env.example) or as an environment variable.");

        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton(BindSignInOptions(configuration));
        services.AddSingleton<CurrentUserSession>();
        services.AddSingleton<ICurrentUser>(sp => sp.GetRequiredService<CurrentUserSession>());
        services.AddSingleton<ICurrentUserSession>(sp => sp.GetRequiredService<CurrentUserSession>());
        services.AddSingleton<AuditLogFactory>();
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();

        // No EnableRetryOnFailure: a retrying strategy rejects the transactions posting operations open themselves.
        services.AddScoped<AuditInterceptor>();
        services.AddDbContext<AppDbContext>((sp, options) => options
            .UseSqlServer(connectionString)
            .AddInterceptors(sp.GetRequiredService<AuditInterceptor>()));
        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<IAuditLogWriter, AuditLogWriter>();
        services.AddScoped<ISchemaVersionChecker, SchemaVersionChecker>();
        services.AddScoped<IDatabaseMigrator, DatabaseMigrator>();
        services.AddScoped<IDemoDataSeeder, DemoDataSeeder>();

        services.AddScoped<IUserStore, UserStore>();
        services.AddScoped<IPermissionChecker, PermissionChecker>();
        services.AddScoped<IFacilityInfoStore, FacilityInfoStore>();
        services.AddScoped<IInmateStore, InmateStore>();
        services.AddScoped<ICustodyVoucherStore, CustodyVoucherStore>();
        services.AddScoped<INumberingService, VoucherCounterNumberingService>();
        services.AddScoped<ICustodyBalanceWriter, CustodyBalanceWriter>();

        // One Infrastructure template per report model; IReportRenderer is the only engine touchpoint.
        services.AddScoped<IReportRenderer, QuestPdfReportRenderer>();
        services.AddScoped<IReportTemplate<DepositReceiptModel>, DepositReceiptTemplate>();

        return services;
    }

    // Bound by hand: Infrastructure has IConfiguration but not the Binder package, and one int needs no binder.
    private static SignInOptions BindSignInOptions(IConfiguration configuration)
    {
        var options = new SignInOptions();
        if (int.TryParse(configuration[$"{SignInOptions.SectionName}:{nameof(SignInOptions.LockoutMinutes)}"], out var minutes))
            options.LockoutMinutes = minutes;

        return options;
    }
}
