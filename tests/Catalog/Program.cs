using Designer.Modules.BetterShortcuts;

int checks = 0;
void Check(bool condition, string message) { if (!condition) throw new Exception(message); checks++; }
var official = ShortcutCatalog.All.Where(r => r.InOfficialTable).ToArray();
Check(official.Length == 60, "The 10.5 official table has 60 rows, including gestures and procedures.");
foreach (var (section, count) in new[] { ("Objects", 27), ("Editing", 10), ("Individual Controls", 6), ("Q-SYS Designs", 17) })
    Check(official.Count(r => r.Section == section) == count, "Missing rows in " + section);
Check(ShortcutCatalog.All.Select(r => r.Id).Distinct().Count() == ShortcutCatalog.All.Length, "Duplicate reference IDs.");
Check(ShortcutCatalog.All.Where(r => r.NativeName is not null).Select(r => (r.NativeOwner, r.NativeName)).Distinct().Count()
    == ShortcutCatalog.All.Count(r => r.NativeName is not null), "Native mappings must be unique.");
Check(ShortcutCatalog.All.All(r => !string.IsNullOrWhiteSpace(r.When) && !string.IsNullOrWhiteSpace(r.Keybinding)
    && new Uri(r.SourceUrl).Host == "help.qsys.com"), "Reference context, binding, or official source missing.");
var empty = ShortcutTable.Build([], new Dictionary<string, string>());
Check(empty.Length == ShortcutCatalog.All.Length + ShortcutTable.InternalGestures.Length && empty.All(r => !r.CanAssign), "Absent native commands must stay visible as references.");
Check(empty.Single(r => r.Id == "internal/application-wheel").Source == "Q-SYS internals (reference)", "Static-only findings must not be labeled as public Help.");
Check(empty.Count(r => r.Command.Contains("intersecting")) == 2, "Distinct selection gestures were collapsed.");
Check(empty.Single(r => r.Id == "help/component-values").Details.Contains("cannot be undone"), "Control paste warning lost.");
Check(empty.Single(r => r.Id == "help/control-position").Keybinding.Contains("Shift+click"), "Coordinate-preserving paste lost.");
var native = new CommandSnapshot("native/save", "Save", "Q-SYS", "Ctrl+S", "Ctrl+S", "Designer", ShortcutCatalog.ApplicationOwner, "Save");
var mod = new CommandSnapshot("example/save", "Save", "Example Mod", "Ctrl+Alt+S", "Ctrl+Alt+S", "Mod enabled");
var extra = new CommandSnapshot("native/extra", "Extra Native Command", "Q-SYS", "", "", "Native context", "QSC.QSys.MainWindow", "Extra");
var commands = new[] { native, mod, extra };
var rows = ShortcutTable.Build(commands, new Dictionary<string, string>());
Check(rows.Single(r => r.Id == "help/save").CommandId == native.Id, "Native identity did not map to the official row.");
Check(rows.Single(r => r.Id == mod.Id).Source == "Example Mod", "Same-label mod must remain separate.");
Check(rows.Count(r => r.CommandId == native.Id) == 1, "Native command duplicated.");
Check(rows.Single(r => r.Id == extra.Id).Keybinding == "Unassigned", "Unbound native actions must be retained.");
var overrides = new Dictionary<string, string> { [native.Id] = "Ctrl+Alt+F11", ["absent/mod"] = "Ctrl+Alt+F12" };
rows = ShortcutTable.Build([native with { Keybinding = "Ctrl+Alt+F11" }, mod], overrides);
Check(rows.Single(r => r.Id == "help/save") is { Source: "User · Q-SYS", Keybinding: "Ctrl+Alt+F11" }, "Staged override display incorrect.");
Check(rows.Single(r => r.Id == "help/save").Details.Contains("Documented: Ctrl+S"), "Override hid the documented default.");
Check(!rows.Any(r => r.CommandId == "absent/mod") && overrides.ContainsKey("absent/mod"), "Absent mod preference was fabricated or lost.");
overrides[native.Id] = "";
rows = ShortcutTable.Build([native with { Keybinding = "" }], overrides);
Check(rows.Single(r => r.Id == "help/save") is { Source: "User · Q-SYS", Keybinding: "Unassigned" }, "Clear must show an unassigned user override.");
overrides.Remove(native.Id);
rows = ShortcutTable.Build([native], overrides);
Check(rows.Single(r => r.Id == "help/save") is { Source: "Q-SYS", Keybinding: "Ctrl+S" }, "Reset must restore provider/default display.");
var ambiguous = ShortcutTable.Build([native, native with { Id = "native/duplicate" }], new Dictionary<string, string>());
Check(!ambiguous.Single(r => r.Id == "help/save").CanAssign, "Ambiguous native identities must not be guessed.");
Check(rows.Single(r => r.Id == "help/save").Matches("save CTRL+S q-sys"), "Search must combine words across columns, case-insensitively.");
Check(rows.Single(r => r.Id == "help/control-position").Matches("coordinates Shift"), "Search must include contextual instructions.");
Check(rows.All(r => !r.Matches("xyz-no-such-shortcut")), "No-match search must be empty.");
Console.WriteLine($"Passed {checks} catalog and projection checks; {official.Length} official rows + {ShortcutCatalog.All.Length - official.Length} additional Help rows + {ShortcutTable.InternalGestures.Length} static native gestures.");
