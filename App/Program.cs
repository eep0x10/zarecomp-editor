namespace ZaTool.App;

internal static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();
        var index = Array.IndexOf(args, "--save");
        Application.Run(new MainWindow(index >= 0 && index + 1 < args.Length ? args[index + 1] : null));
    }
}
