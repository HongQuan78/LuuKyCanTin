namespace LuuKyCanTin.WinForms.Shell;

/// <summary>The letters on the user's initials tile: first and last word of a full name, or the first two letters of one word.</summary>
public static class UserInitials
{
    public static string Create(string name)
    {
        var words = name.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var initials = words.Length switch
        {
            0 => "",
            1 => words[0][..Math.Min(2, words[0].Length)],
            _ => $"{words[0][0]}{words[^1][0]}",
        };
        return initials.ToUpperInvariant();
    }
}
