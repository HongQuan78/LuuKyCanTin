using LuuKyCanTin.Application.Abstractions;
using LuuKyCanTin.Application.BaoCao;
using Microsoft.Extensions.DependencyInjection;

namespace LuuKyCanTin.Infrastructure.Reports;

/// <summary>Resolves the template for the model type and returns the PDF bytes. The engine stays behind this port.</summary>
internal sealed class QuestPdfReportRenderer(IServiceProvider services) : IReportRenderer
{
    public byte[] Render<TModel>(TModel model)
        where TModel : IReportModel
        => services.GetRequiredService<IReportTemplate<TModel>>().Render(model);
}
