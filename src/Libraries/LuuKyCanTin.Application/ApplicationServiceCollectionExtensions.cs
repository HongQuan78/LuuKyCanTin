using FluentValidation;
using LuuKyCanTin.Application.Administration;
using LuuKyCanTin.Application.Custody;
using LuuKyCanTin.Application.MasterData;
using LuuKyCanTin.Application.Reporting;
using Microsoft.Extensions.DependencyInjection;

namespace LuuKyCanTin.Application;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton<IValidator<SaveOfficerRequest>, SaveOfficerRequestValidator>();
        services.AddScoped<IOfficerService, OfficerService>();
        services.AddScoped<IRoleService, RoleService>();

        services.AddScoped<FailedSignInService>();
        services.AddScoped<SignInService>();
        services.AddScoped<ChangePasswordService>();
        services.AddScoped<AddInmateService>();
        services.AddScoped<InmatesInCustodyQuery>();
        services.AddScoped<CustodyLedgerService>();
        services.AddScoped<DepositReceiptPrintQuery>();

        return services;
    }
}
