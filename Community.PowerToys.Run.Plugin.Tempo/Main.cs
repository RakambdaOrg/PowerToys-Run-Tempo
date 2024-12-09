using ManagedCommon;
using Wox.Plugin;
using Wox.Plugin.Logger;

namespace PowerToys_Run_Tempo;

public class Main : IPlugin, IDisposable
{
    public static string PluginID => "07C11C587B744730A9D8CBE30B1106E4";
    public string Name => "Tempo";
    public string Description => "Log your Tempo time";

    private PluginInitContext? Context { get; set; }
    private string? PluginPath { get; set; }
    private string? IconPath { get; set; }
    private bool Disposed { get; set; }

    public List<Result> Query(Query query)
    {
        return [];
    }

    public void Init(PluginInitContext context)
    {
        Log.Info("Init", GetType());
        Context = context ?? throw new ArgumentNullException(nameof(context));
        Context.API.ThemeChanged += OnThemeChanged;
        UpdateIconPath(Context.API.GetCurrentTheme());

        InitPluginPath(Context.CurrentPluginMetadata.PluginDirectory);
    }

    public void InitPluginPath(string path)
    {
        PluginPath = path;
    }

    public void Dispose()
    {
        Log.Info("Dispose", GetType());
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (Disposed || !disposing)
        {
            return;
        }

        if (Context?.API != null)
        {
            Context.API.ThemeChanged -= OnThemeChanged;
        }

        Disposed = true;
    }

    private void UpdateIconPath(Theme theme) => IconPath = theme is Theme.Light or Theme.HighContrastWhite ? Context?.CurrentPluginMetadata.IcoPathLight : Context?.CurrentPluginMetadata.IcoPathDark;

    private void OnThemeChanged(Theme currentTheme, Theme newTheme) => UpdateIconPath(newTheme);
}