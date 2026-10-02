namespace LuuKyCanTin.WinForms.Shell;

public interface IMainView
{
    event EventHandler Loaded;

    event EventHandler DanhMucCanBoClicked;

    string TieuDe { set; }
}
