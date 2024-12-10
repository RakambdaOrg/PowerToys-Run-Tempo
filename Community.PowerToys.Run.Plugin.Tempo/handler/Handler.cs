using Wox.Plugin;

namespace PowerToys_Run_Tempo.handler;

public interface IHandler
{
    List<Result> HandleQuery(Query query);

    Result GenerateUsageResult();
}