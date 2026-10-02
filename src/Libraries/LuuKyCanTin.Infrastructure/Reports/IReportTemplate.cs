using LuuKyCanTin.Application.BaoCao;

namespace LuuKyCanTin.Infrastructure.Reports;

/// <summary>One PDF template per report model. Registered in DI and resolved by <see cref="QuestPdfReportRenderer"/>.</summary>
internal interface IReportTemplate<in TModel>
    where TModel : IReportModel
{
    byte[] Render(TModel model);
}
