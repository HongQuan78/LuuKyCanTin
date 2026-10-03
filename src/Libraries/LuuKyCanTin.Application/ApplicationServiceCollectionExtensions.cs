using FluentValidation;
using LuuKyCanTin.Application.BaoCao;
using LuuKyCanTin.Application.DanhMuc;
using LuuKyCanTin.Application.HeThong;
using LuuKyCanTin.Application.LuuKy;
using Microsoft.Extensions.DependencyInjection;

namespace LuuKyCanTin.Application;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton<IValidator<LuuCanBoRequest>, LuuCanBoRequestValidator>();
        services.AddScoped<ICanBoService, CanBoService>();
        services.AddScoped<IVaiTroService, VaiTroService>();

        services.AddScoped<GhiNhanDangNhapSaiService>();
        services.AddScoped<DangNhapService>();
        services.AddScoped<DoiMatKhauService>();
        services.AddScoped<ThemDoiTuongService>();
        services.AddScoped<LayDoiTuongDangQuanLyQuery>();
        services.AddScoped<GhiSoLuuKyService>();
        services.AddScoped<LayBienNhanThuDeInQuery>();

        return services;
    }
}
