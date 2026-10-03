using LuuKyCanTin.Application.Administration;
using LuuKyCanTin.Application.Common;
using LuuKyCanTin.Domain.Administration;
using LuuKyCanTin.WinForms.Common;
using Shouldly;

namespace LuuKyCanTin.WinForms.UnitTests.Common;

/// <summary>A business error gets a warning with its own message; anything else keeps the unexpected-error path.</summary>
public class GlobalExceptionHandlerTests
{
    [Fact]
    public void IsBusinessError_BusinessRuleException_IsTrue() =>
        GlobalExceptionHandler.IsBusinessError(new BusinessRuleException("Số tiền phải lớn hơn 0.")).ShouldBeTrue();

    [Fact]
    public void IsBusinessError_PermissionDeniedException_IsTrue() =>
        GlobalExceptionHandler.IsBusinessError(new PermissionDeniedException("DM.Them")).ShouldBeTrue();

    [Fact]
    public void IsBusinessError_SeparationOfDutiesViolationException_IsTrue() =>
        GlobalExceptionHandler.IsBusinessError(new SeparationOfDutiesViolationException()).ShouldBeTrue();

    [Fact]
    public void IsBusinessError_AggregateWrappingABusinessError_IsTrue() =>
        GlobalExceptionHandler
            .IsBusinessError(new AggregateException(new PermissionDeniedException(PermissionCodes.MasterData.Create)))
            .ShouldBeTrue();

    [Fact]
    public void IsBusinessError_UnexpectedException_IsFalse() =>
        GlobalExceptionHandler.IsBusinessError(new InvalidOperationException("db down")).ShouldBeFalse();

    [Fact]
    public void IsBusinessError_NoException_IsFalse() =>
        GlobalExceptionHandler.IsBusinessError(null).ShouldBeFalse();

    [Fact]
    public void TryShowBusinessWarning_PermissionDenied_ShowsTheWarningTitleAndItsOwnMessage()
    {
        var shown = new List<(string Message, string Title)>();
        var original = GlobalExceptionHandler.ShowWarning;
        try
        {
            GlobalExceptionHandler.ShowWarning = (message, title) => shown.Add((message, title));
            var denied = new PermissionDeniedException(PermissionCodes.MasterData.Create);

            GlobalExceptionHandler.TryShowBusinessWarning(denied).ShouldBeTrue();

            shown.ShouldHaveSingleItem();
            shown[0].Title.ShouldBe("Cảnh báo");
            shown[0].Message.ShouldBe(denied.Message);
        }
        finally
        {
            GlobalExceptionHandler.ShowWarning = original;
        }
    }

    [Fact]
    public void TryShowBusinessWarning_SeparationOfDutiesViolation_ShowsTheWarningTitleAndItsOwnMessage()
    {
        var shown = new List<(string Message, string Title)>();
        var original = GlobalExceptionHandler.ShowWarning;
        try
        {
            GlobalExceptionHandler.ShowWarning = (message, title) => shown.Add((message, title));
            var violation = new SeparationOfDutiesViolationException();

            GlobalExceptionHandler.TryShowBusinessWarning(violation).ShouldBeTrue();

            shown.ShouldHaveSingleItem();
            shown[0].Title.ShouldBe("Cảnh báo");
            shown[0].Message.ShouldBe(violation.Message);
        }
        finally
        {
            GlobalExceptionHandler.ShowWarning = original;
        }
    }

    [Fact]
    public void TryShowBusinessWarning_UnexpectedException_ReturnsFalseAndShowsNothing()
    {
        var shown = 0;
        var original = GlobalExceptionHandler.ShowWarning;
        try
        {
            GlobalExceptionHandler.ShowWarning = (_, _) => shown++;

            GlobalExceptionHandler.TryShowBusinessWarning(new InvalidOperationException("db down")).ShouldBeFalse();

            shown.ShouldBe(0);
        }
        finally
        {
            GlobalExceptionHandler.ShowWarning = original;
        }
    }
}
