namespace LuuKyCanTin.Application.BaoCao;

/// <summary>One plain model per printed template, holding only snapshotted data (never a live query).</summary>
public interface IReportModel
{
    /// <summary>Matches <c>CauHinhKyTen.MaMauIn</c>, e.g. <c>BIEN_NHAN_THU</c>.</summary>
    string MaMauIn { get; }
}
