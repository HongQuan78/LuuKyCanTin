namespace LuuKyCanTin.Application.MasterData;

public sealed record AddInmateResult(bool Succeeded, int Id, string? Message)
{
    public static AddInmateResult Ok(int id) => new(true, id, null);

    public static AddInmateResult Fail(string message) => new(false, 0, message);
}
