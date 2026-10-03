namespace LuuKyCanTin.WinForms.Shell;

/// <summary>The shell window: owner of the dialogs it opens, and the content area that hosts cached screens.</summary>
public interface IContentHost : IWin32Window
{
    /// <inheritdoc cref="INavigator.ShowPage"/>
    void ShowPage(string key, string title, Func<Control> create);
}
