using System.Runtime.InteropServices;

namespace SP03Search;

internal static class ParentConsole
{
    private const int AttachParentProcess = -1;

    public static bool TryAttach() => AttachConsole(AttachParentProcess);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool AttachConsole(int processId);
}
