namespace PowerToys_Run_Tempo.jira.api.model;

public class IssueInfoResponse(
    string id,
    string key,
    Fields fields
)
{
    public string id { get; init; } = id;
    public string key { get; init; } = key;
    public Fields fields { get; init; } = fields;
}