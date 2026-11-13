using System.Diagnostics;

namespace ZaTool.WebHost;

public static class Program
{
    public static async Task Main(string[] args)
    {
        string? Value(string key) { int i = Array.IndexOf(args, key); return i >= 0 && i + 1 < args.Length ? args[i + 1] : null; }
        var options = new HostOptions
        {
            InitialPath = Value("--save"),
            Port = int.TryParse(Value("--port"), out var port) ? port : 0,
            RuntimeFile = Value("--runtime-file"),
            IdleTimeout = args.Contains("--no-browser") ? TimeSpan.Zero : TimeSpan.FromMinutes(5)
        };
        await using var host = new WorkbenchHost(options);
        await host.StartAsync();
        if (!args.Contains("--no-browser"))
        {
            try { Process.Start(new ProcessStartInfo(host.BrowserUrl) { UseShellExecute = true }); }
            catch { await host.WriteRuntimeFileAsync(Path.Combine(AppContext.BaseDirectory, "RecompPokemon.runtime.json")); }
        }
        await host.WaitForShutdownAsync();
    }
}
