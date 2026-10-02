using FluentValidation;
using LuuKyCanTin.Application.DanhMuc;
using Microsoft.Extensions.DependencyInjection;

namespace LuuKyCanTin.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton<IValidator<LuuCanBoRequest>, LuuCanBoRequestValidator>();
        services.AddScoped<ICanBoService, CanBoService>();
        return services;
    }
}
