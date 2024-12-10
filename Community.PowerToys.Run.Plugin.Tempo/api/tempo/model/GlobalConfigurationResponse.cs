namespace PowerToys_Run_Tempo.api.tempo.model;

public class GlobalConfigurationResponse(
    int maxHoursPerDayPerUser
)
{
    public int maxHoursPerDayPerUser { get; init; } = maxHoursPerDayPerUser;
}