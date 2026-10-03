using System.Runtime.InteropServices;

namespace LuuKyCanTin.WinForms.Common;

/// <summary>
/// A WinExe has no console of its own. Attaching to the console of the shell that started it lets an admin
/// running a command-line verb see the output.
/// </summary>
internal static class ParentConsole
{
    private const int AttachParentProcess = -1;

    /// <returns>False when there is no parent console, for example when started from a shortcut.</returns>
    public static bool TryAttach() => AttachConsole(AttachParentProcess);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool AttachConsole(int processId);
}
