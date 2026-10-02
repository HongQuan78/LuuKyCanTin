using LuuKyCanTin.Application.BaoCao;

namespace LuuKyCanTin.Application.Abstractions;

/// <summary>
/// Renders a report model to PDF bytes. One Infrastructure template class per model keeps the engine out of
/// Application, so swapping QuestPDF for RDLC later touches only Infrastructure.
/// </summary>
public interface IReportRenderer
{
    byte[] Render<TModel>(TModel model)
        where TModel : IReportModel;
}
