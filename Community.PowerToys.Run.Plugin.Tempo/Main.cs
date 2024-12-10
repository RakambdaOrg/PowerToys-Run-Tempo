using System.Windows.Controls;
using ManagedCommon;
using Microsoft.PowerToys.Settings.UI.Library;
using PowerToys_Run_Tempo.handler;
using PowerToys_Run_Tempo.jira.api;
using PowerToys_Run_Tempo.tempo.api;
using Wox.Infrastructure.Storage;
using Wox.Plugin;

namespace PowerToys_Run_Tempo;

public class Main : IPlugin, IDisposable, ISettingProvider
{
    public static string PluginID => "07C11C587B744730A9D8CBE30B1106E4";
    public string Name => "Tempo";
    public string Description => "Log your Tempo time";

    private PluginInitContext? Context { get; set; }
    private string? IconPath { get; set; }
    private bool Disposed { get; set; }
    private PluginJsonStorage<Settings> Storage { get; set; }
    private Settings Settings { get; set; }
    public IEnumerable<PluginAdditionalOption> AdditionalOptions => Settings.GetAdditionalOptions();

    private string _accountId;
    private TempoApi _tempoApi;
    private JiraApi _jiraApi;

    private Dictionary<string, IHandler> _handlers;

    public Main()
    {
        Storage = new PluginJsonStorage<Settings>();
        Settings = Storage.Load();
        LogWrapper.Info($"Created instance of {Name} plugin and loaded settings {Settings}", GetType());
    }

    //For tests
    public Main(Settings settings)
    {
        Settings = settings;
        LogWrapper.Info($"Created instance of {Name} plugin and loaded provided settings {settings}", GetType());
    }

    public List<Result> Query(Query query)
    {
        if (query.Terms.Count == 0)
        {
            return GenerateAllHandlersUsage();
        }

        _handlers.TryGetValue(query.Terms[0], out var handler);
        if (handler == null)
        {
            return GenerateAllHandlersUsage();
        }

        LogWrapper.Debug($"Got a recognized command that will be handled by {handler.GetType()}", GetType());
        try
        {
            return handler.HandleQuery(query);
        }
        catch (Exception e)
        {
            LogWrapper.Error($"Handler threw an error {e}", GetType());
            return
            [
                new Result
                {
                    Title = "Error",
                    SubTitle = "Handler error"
                }
            ];
        }
    }

    private List<Result> GenerateAllHandlersUsage()
    {
        return _handlers.Values.Select(h => h.GenerateUsageResult()).ToList();
    }

    public void Init(PluginInitContext context)
    {
        LogWrapper.Info($"Initializing plugin {Name}", GetType());
        Context = context ?? throw new ArgumentNullException(nameof(context));
        Context.API.ThemeChanged += OnThemeChanged;
        UpdateIconPath(Context.API.GetCurrentTheme());

        InitSelf();
    }

    public void InitSelf()
    {
        LogWrapper.Info($"Initializing {Name} plugin components with settings {Settings}", GetType());
        _accountId = Settings.AccountId ?? "";
        _jiraApi = new JiraApi(
            Settings.JiraEndpoint ?? "",
            Settings.JiraUsername ?? "",
            Settings.JiraToken ?? ""
        );
        _tempoApi = new TempoApi(Settings.TempoToken ?? "");

        _handlers = new Dictionary<string, IHandler>
        {
            {
                "log", new LogHandler(_accountId, _jiraApi, _tempoApi)
            }
        };

        LogWrapper.Info($"Done creating components, got {_handlers.Keys} as command handlers", GetType());
    }

    public void Dispose()
    {
        LogWrapper.Info("Dispose", GetType());
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

    public Control CreateSettingPanel()
    {
        throw new NotImplementedException();
    }

    public void UpdateSettings(PowerLauncherPluginSettings settings)
    {
        LogWrapper.Info("Updated settings, re-initializing components", GetType());
        Settings.SetAdditionalOptions(settings.AdditionalOptions);
        InitSelf();
    }
}