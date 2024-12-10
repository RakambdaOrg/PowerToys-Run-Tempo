namespace PowerToys_Run_Tempo.api.tempo.model.model;

public class AddWorklogSchema(
    string authorAccountId,
    long issueId,
    string startDate,
    string startTime,
    long timeSpentSeconds
)
{
    public string authorAccountId { get; init; } = authorAccountId;
    public long issueId { get; init; } = issueId;
    public string startDate { get; init; } = startDate;
    public string startTime { get; init; } = startTime;
    public long timeSpentSeconds { get; init; } = timeSpentSeconds;
}