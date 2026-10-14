using System.Text.RegularExpressions;

namespace Designer.Modules.BetterShortcuts;

internal sealed record KeybindingPart(string Text, bool IsKey);
internal sealed record KeybindingLine(KeybindingPart[] Parts)
{
    public string Text => string.Concat(Parts.Select(p => p.Text));
}

// Display only. Native identity, routing and saved settings continue to use WPF
// keys/modifiers. In particular, number-row and numeric-keypad keys stay distinct.
internal static class KeybindingPresentation
{
    private static readonly Dictionary<string, string> Names = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Back"] = "Backspace", ["Backspace"] = "Backspace", ["Return"] = "Enter", ["Enter"] = "Enter",
        ["Esc"] = "Escape", ["Escape"] = "Escape", ["Del"] = "Delete", ["Delete"] = "Delete",
        ["Ins"] = "Insert", ["Insert"] = "Insert", ["Prior"] = "Page Up", ["PageUp"] = "Page Up",
        ["Next"] = "Page Down", ["PageDown"] = "Page Down", ["Space"] = "Space", ["Tab"] = "Tab",
        ["Home"] = "Home", ["End"] = "End", ["Left"] = "Left Arrow", ["Right"] = "Right Arrow",
        ["Up"] = "Up Arrow", ["Down"] = "Down Arrow", ["Arrow keys"] = "Arrow keys",
        ["OemPlus"] = "=", ["OemMinus"] = "-", ["OemComma"] = ",", ["OemPeriod"] = ".",
        ["Oem1"] = ";", ["OemSemicolon"] = ";", ["Oem2"] = "/", ["OemQuestion"] = "/",
        ["Oem3"] = "`", ["OemTilde"] = "`", ["Oem4"] = "[", ["OemOpenBrackets"] = "[",
        ["Oem5"] = "\\", ["OemPipe"] = "\\", ["Oem6"] = "]", ["OemCloseBrackets"] = "]",
        ["Oem7"] = "'", ["OemQuotes"] = "'", ["Oem102"] = "Intl \\", ["OemBackslash"] = "Intl \\",
        ["Add"] = "NumPad +", ["Subtract"] = "NumPad -", ["Multiply"] = "NumPad *",
        ["Divide"] = "NumPad /", ["Decimal"] = "NumPad .", ["Separator"] = "NumPad Separator",
        ["click"] = "Click", ["drag"] = "Drag", ["mouse wheel"] = "Mouse Wheel"
    };

    public static KeybindingLine[] Lines(string value, bool reference)
    {
        if (reference)
            return value.Split(" / ", StringSplitOptions.TrimEntries).Select(ReferenceLine).ToArray();
        return value.Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Select(v => Chord(v) ?? new[] { new KeybindingPart(v, false) })
            .Select(parts => new KeybindingLine(parts))
            .DistinctBy(line => line.Text, StringComparer.OrdinalIgnoreCase).ToArray();
    }

    public static string Text(string value, bool reference) => string.Join(" or ", Lines(value, reference).Select(line => line.Text));

    private static KeybindingLine ReferenceLine(string value)
    {
        var parts = new List<KeybindingPart>();
        // Semicolons in reference procedures mean successive steps, whereas
        // semicolons in registered commands separate alternative bindings.
        foreach (var segment in Regex.Split(value, @"(, then |; | while )"))
        {
            if (segment is ", then " or "; ") parts.Add(new(" then ", false));
            else if (segment == " while ") parts.Add(new(segment, false));
            else if (Chord(segment) is { } chord) parts.AddRange(chord);
            else
            {
                var drag = Regex.Match(segment, @"^(?<mods>(?:(?:Ctrl|Alt|Shift)\+)+)drag(?<direction> selection .+)$");
                if (drag.Success)
                {
                    parts.AddRange(Chord(drag.Groups["mods"].Value + "drag")!);
                    parts.Add(new(drag.Groups["direction"].Value, false));
                }
                else parts.Add(new(segment, false));
            }
        }
        return new(parts.ToArray());
    }

    private static KeybindingPart[]? Chord(string value)
    {
        var modifiers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        string key = value.Trim();
        while (Regex.Match(key, @"^(Ctrl|Control|Alt|Shift|Win|Windows)\+", RegexOptions.IgnoreCase) is { Success: true } match)
        {
            modifiers.Add(match.Groups[1].Value.ToLowerInvariant() switch { "control" => "ctrl", "windows" => "win", var modifier => modifier });
            key = key[match.Length..];
        }
        string label;
        if (Names.TryGetValue(key, out var name)) label = name;
        else if (Regex.IsMatch(key, @"^NumPad[0-9]$", RegexOptions.IgnoreCase)) label = "NumPad " + key[^1];
        else if (Regex.IsMatch(key, @"^D[0-9]$", RegexOptions.IgnoreCase)) label = key[1..];
        else if (Regex.IsMatch(key, @"^(?:[A-Za-z0-9]|F(?:[1-9]|1[0-9]|2[0-4])|[=+\-\[\];,./\\'`])$", RegexOptions.IgnoreCase)) label = key.ToUpperInvariant();
        else if (modifiers.Count > 0 && !key.Contains(' ') && !key.Contains(',')) label = key;
        else return null;
        var keys = new[] { "Ctrl", "Shift", "Alt", "Win" }.Where(m => modifiers.Contains(m)).Append(label).ToArray();
        var parts = new List<KeybindingPart>();
        foreach (var token in keys)
        {
            if (parts.Count > 0) parts.Add(new(" + ", false));
            parts.Add(new(token, true));
        }
        return parts.ToArray();
    }
}
