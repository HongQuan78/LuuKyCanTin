using FluentValidation;
using LuuKyCanTin.Application.BaoCao;
using LuuKyCanTin.Application.DanhMuc;
using LuuKyCanTin.Application.HeThong;
using LuuKyCanTin.Application.LuuKy;
using Microsoft.Extensions.DependencyInjection;

namespace LuuKyCanTin.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton<IValidator<LuuCanBoRequest>, LuuCanBoRequestValidator>();
        services.AddScoped<ICanBoService, CanBoService>();

        services.AddScoped<DangNhapService>();
        services.AddScoped<ThemDoiTuongService>();
        services.AddScoped<LayDoiTuongDangQuanLyQuery>();
        services.AddScoped<GhiSoLuuKyService>();
        services.AddScoped<LayBienNhanThuDeInQuery>();

        return services;
    }
}
