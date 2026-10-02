using LuuKyCanTin.Application.Abstractions;
using LuuKyCanTin.Application.BaoCao;
using LuuKyCanTin.Application.DanhMuc;
using LuuKyCanTin.Application.HeThong;
using LuuKyCanTin.Application.LuuKy;
using LuuKyCanTin.Infrastructure.Common;
using LuuKyCanTin.Infrastructure.DanhMuc;
using LuuKyCanTin.Infrastructure.HeThong;
using LuuKyCanTin.Infrastructure.LuuKy;
using LuuKyCanTin.Infrastructure.Persistence;
using LuuKyCanTin.Infrastructure.Persistence.Interceptors;
using LuuKyCanTin.Infrastructure.Persistence.Seed.Demo;
using LuuKyCanTin.Infrastructure.Reports;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LuuKyCanTin.Infrastructure;

public static class DependencyInjection
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
        services.AddSingleton<CurrentUserSession>();
        services.AddSingleton<ICurrentUser>(sp => sp.GetRequiredService<CurrentUserSession>());
        services.AddSingleton<ICurrentUserSession>(sp => sp.GetRequiredService<CurrentUserSession>());
        services.AddSingleton<NhatKyFactory>();
        services.AddSingleton<IMatKhauHasher, Pbkdf2MatKhauHasher>();

        // No EnableRetryOnFailure: a retrying strategy rejects the transactions posting operations open themselves.
        services.AddScoped<AuditInterceptor>();
        services.AddDbContext<AppDbContext>((sp, options) => options
            .UseSqlServer(connectionString)
            .AddInterceptors(sp.GetRequiredService<AuditInterceptor>()));
        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<IGhiNhatKy, GhiNhatKy>();
        services.AddScoped<ISchemaVersionChecker, SchemaVersionChecker>();
        services.AddScoped<IDatabaseMigrator, DatabaseMigrator>();
        services.AddScoped<IDemoDataSeeder, DemoDataSeeder>();

        services.AddScoped<INguoiDungStore, NguoiDungStore>();
        services.AddScoped<IThongTinDonViStore, ThongTinDonViStore>();
        services.AddScoped<IDoiTuongStore, DoiTuongStore>();
        services.AddScoped<IChungTuLuuKyStore, ChungTuLuuKyStore>();
        services.AddScoped<INumberingService, DemSoChungTuNumberingService>();
        services.AddScoped<ISoDuLuuKyWriter, SoDuLuuKyWriter>();

        // One Infrastructure template per report model; IReportRenderer is the only engine touchpoint.
        services.AddScoped<IReportRenderer, QuestPdfReportRenderer>();
        services.AddScoped<IReportTemplate<BienNhanThuModel>, BienNhanThuReport>();

        return services;
    }
}
