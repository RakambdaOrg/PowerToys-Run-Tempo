using PowerToys_Run_Tempo;

namespace PowerToys_Run_Tempo_UnitTest;

public class TestUtils
{
    public static void MockLogs()
    {
        LogWrapper.Debug = (_, _) => { };
        LogWrapper.Info = (_, _) => { };
        LogWrapper.Error = (_, _) => { };
    }
}