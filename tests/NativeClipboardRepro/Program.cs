using System.Reflection;
using System.Runtime.Loader;
using System.Text.Json;
using System.IO;

internal static class Program
{
    [STAThread] private static void Main(string[] args)
    {
        if (args.Length != 1) throw new ArgumentException("Pass the installed Designer 10.5 directory. This manual test replaces clipboard contents with synthetic data.");
        string directory = args[0];
        string designerPath = Path.Combine(directory, "Q-Sys Designer.dll");
        using (var file = File.OpenRead(designerPath))
            if (Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(file)) != "0E7150F8DC02E0159E019188780FB003CE025F5EC18C491609CF337D849BDA60")
                throw new InvalidOperationException("Unexpected Designer binary; the reproduction is scoped to the tested 10.5.0 build.");
        AssemblyLoadContext.Default.Resolving += (_, name) => {
            var path = Path.Combine(directory, name.Name + ".dll");
            return File.Exists(path) ? AssemblyLoadContext.Default.LoadFromAssemblyPath(path) : null;
        };
        var designer = AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.Combine(directory, "Q-Sys Designer.dll"));
        var common = AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.Combine(directory, "QscCommon.dll"));
        var type = designer.GetType("QSC.QSys.Commands.CopyControlSet", true)!;
        var clipboard = common.GetType("QSC.Common.Clipboard", true)!;
        var value = Activator.CreateInstance(type)!;
        type.GetProperty("ClassName")!.SetValue(value, "gain");
        type.GetProperty("Controls")!.SetValue(value, new Dictionary<string, string> { ["mute"] = "0" });
        clipboard.GetMethod("SetData")!.Invoke(null, ["QSys.CopyControls", value, true]);
        var present = clipboard.GetMethod("GetDataPresent", [typeof(string)])!.Invoke(null, ["QSys.CopyControls"]);
        var read = clipboard.GetMethods().Single(m => m.Name == "GetData" && m.IsGenericMethod && m.GetParameters().Length == 1)
            .MakeGenericMethod(type).Invoke(null, ["QSys.CopyControls"]);
        // Deliberately inspect the same legacy clipboard path used by the host.
#pragma warning disable WFDEV005
        var raw = System.Windows.Forms.Clipboard.GetData("QSys.CopyControls");
#pragma warning restore WFDEV005
        System.Windows.Forms.Clipboard.SetText("synthetic-clipboard-sanity");
        Console.WriteLine(JsonSerializer.Serialize(new {
            runtime = Environment.Version.ToString(), formatPresent = present, typedReadSucceeded = read is not null,
            rawType = raw?.GetType().FullName, rawError = (raw as Exception)?.ToString(), plainTextRoundTrip = System.Windows.Forms.Clipboard.GetText() == "synthetic-clipboard-sanity",
            modsLoaded = AppDomain.CurrentDomain.GetAssemblies().Any(a => a.GetName().Name?.Contains("ModsCore") == true || a.GetName().Name?.Contains("BetterShortcuts") == true || a.GetName().Name?.StartsWith("Qmm.") == true)
        }));
    }
}
