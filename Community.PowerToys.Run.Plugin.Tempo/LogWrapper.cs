using Wox.Plugin.Logger;

namespace PowerToys_Run_Tempo;

public static class LogWrapper
{
    public static Action<string, Type> Debug = (message, type) => Log.Debug(message, type);
    public static Action<string, Type> Info = (message, type) => Log.Info(message, type);
    public static Action<string, Type> Error = (message, type) => Log.Error(message, type);
}