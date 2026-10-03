using System.Reflection;
using LuuKyCanTin.Application.Abstractions;
using LuuKyCanTin.Application.Administration;
using LuuKyCanTin.Application.MasterData;
using Shouldly;

namespace LuuKyCanTin.IntegrationTests.Architecture;

/// <summary>
/// Every Application service that can write must depend on <see cref="IPermissionChecker"/>; later epics inherit
/// the guard for free, so a new write service can't ship without an authorization check.
/// </summary>
public class ServiceAuthorizationTests
{
    // Sign-in, change-password and the failed-attempt helper act only on the caller's own credentials,
    // so they need no permission; every other write service re-checks the database.
    private static readonly Type[] AllowedWithoutPermissionChecker =
    [
        typeof(SignInService),
        typeof(ChangePasswordService),
        typeof(FailedSignInService),
    ];

    [Fact]
    public void EveryApplicationWriteService_TakesAnIPermissionChecker()
    {
        var offenders = typeof(OfficerService).Assembly.GetTypes()
            .Where(type => type.IsClass && type.IsPublic && !type.IsAbstract)
            .Where(type => type.Name.EndsWith("Service", StringComparison.Ordinal))
            .Where(HasAWriteMethod)
            .Where(type => !AllowedWithoutPermissionChecker.Contains(type))
            .Where(type => !type.GetConstructors().Any(HasPermissionChecker))
            .Select(type => type.Name)
            .ToList();

        offenders.ShouldBeEmpty();
    }

    private static bool HasAWriteMethod(Type type) =>
        type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Any(method => !method.IsSpecialName
                && !method.Name.StartsWith("Get", StringComparison.Ordinal)
                && !method.Name.StartsWith("Search", StringComparison.Ordinal));

    private static bool HasPermissionChecker(ConstructorInfo constructor) =>
        constructor.GetParameters().Any(parameter => parameter.ParameterType == typeof(IPermissionChecker));
}
