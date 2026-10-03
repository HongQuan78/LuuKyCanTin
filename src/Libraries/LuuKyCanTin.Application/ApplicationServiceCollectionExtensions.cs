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
        services.AddSingleton<IValidator<CreateAccountRequest>, CreateAccountRequestValidator>();
        services.AddScoped<IOfficerService, OfficerService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<LastAdministratorGuard>();
        services.AddScoped<IAccountService, AccountService>();

        services.AddScoped<FailedSignInService>();
        services.AddScoped<SignInService>();
        services.AddScoped<ChangePasswordService>();
        services.AddScoped<ISignedInUserQuery, SignedInUserQuery>();
        services.AddScoped<SeparationOfDutiesPolicy>();
        services.AddScoped<AddInmateService>();
        services.AddScoped<InmatesInCustodyQuery>();
        services.AddScoped<CustodyLedgerService>();
        services.AddScoped<DepositReceiptPrintQuery>();

        return services;
    }
}
