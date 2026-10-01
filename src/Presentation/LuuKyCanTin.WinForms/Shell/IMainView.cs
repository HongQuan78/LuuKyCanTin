namespace LuuKyCanTin.WinForms.Shell;

public interface IMainView
{
    event EventHandler Loaded;

    string TieuDe { set; }
}
