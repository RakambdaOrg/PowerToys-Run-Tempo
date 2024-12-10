using System.Globalization;
using System.Text.RegularExpressions;
using PowerToys_Run_Tempo.api.tempo.model.model;
using PowerToys_Run_Tempo.jira.api;
using PowerToys_Run_Tempo.jira.api.model;
using PowerToys_Run_Tempo.tempo.api;
using Wox.Plugin;
using Wox.Plugin.Logger;

namespace PowerToys_Run_Tempo.handler;

public partial class LogHandler(
    string accountId,
    JiraApi jiraApi,
    TempoApi tempoApi
) : IHandler
{
    private static readonly Regex DurationRegex = DurationRegexGenerator();
    private static readonly Regex IssueKeyRegex = IssueKeyRegexGenerator();
    private string AccountId { get; } = accountId;
    private JiraApi JiraApi { get; } = jiraApi;
    private TempoApi TempoApi { get; } = tempoApi;

    [GeneratedRegex(@"^(?:(?<hours>\d+)[Hh])?(?:(?<minutes>\d+)[Mm])?(?:(?<seconds>\d+)[Ss])?$")]
    private static partial Regex DurationRegexGenerator();

    [GeneratedRegex(@"^\w+-\d+$")]
    private static partial Regex IssueKeyRegexGenerator();

    private readonly Dictionary<string, IssueInfoResponse> _jiraIssueCache = new();

    public List<Result> HandleQuery(Query query)
    {
        if (query.Terms.Count < 3)
        {
            return [GenerateUsageResult()];
        }

        var issueKey = query.Terms[1]!;
        var duration = query.Terms[2]!;

        if (!IssueKeyRegex.IsMatch(issueKey))
        {
            return [GenerateUsageResult()];
        }

        IssueInfoResponse? issueInfo;
        _jiraIssueCache.TryGetValue(issueKey, out issueInfo);
        if (issueInfo == null)
        {
            try
            {
                issueInfo = JiraApi.GetIssueInfo(issueKey);
            }
            catch (Exception e)
            {
                LogWrapper.Error($"Jira API error {e}", GetType());
                return GenerateErrorResponse("Error while contacting Jira API");
            }
        }

        if (issueInfo == null)
        {
            return GenerateErrorResponse("Failed to get issue ID from Jira API");
        }

        _jiraIssueCache.TryAdd(issueKey, issueInfo);

        var timeSpan = ExtractPeriod(duration);
        if (timeSpan == null)
        {
            return GenerateErrorResponse("Failed to parse duration");
        }

        var timeSpentSeconds = (long)timeSpan?.TotalSeconds!;
        var startDate = (query.Terms.Count >= 4 ? ExtractDate(query.Terms[3]) : DateTime.Today).ToString("yyyy-MM-dd");
        var startTime = (query.Terms.Count >= 5 ? ExtractTime(query.Terms[4]) : DateTime.Now).ToString("HH:mm:ss");

        var schema = new AddWorklogSchema(
            AccountId,
            long.Parse(issueInfo.id),
            startDate,
            startTime,
            timeSpentSeconds
        );

        return
        [
            new Result
            {
                Title = $"Log time on {issueKey} at {startDate} {startTime} for {duration}",
                SubTitle = issueInfo.fields.summary,
                Action = _ => SendWorklog(schema)
            }
        ];
    }

    private bool SendWorklog(AddWorklogSchema schema)
    {
        try
        {
            var result = TempoApi.AddWorklog(schema);
            if (result is { tempoWorklogId: > 0 })
            {
                return true;
            }
        }
        catch (Exception e)
        {
            LogWrapper.Error($"Tempo API error {e}", GetType());
        }

        return false;
    }

    private static TimeSpan? ExtractPeriod(string duration)
    {
        if (duration == "")
        {
            return null;
        }

        var match = DurationRegex.Match(duration);
        if (match == Match.Empty)
        {
            return null;
        }

        var timeSpan = TimeSpan.Zero;
        if (match.Groups.TryGetValue("hours", out var hours) && hours.Length > 0)
        {
            timeSpan = timeSpan.Add(new TimeSpan(int.Parse(hours.Value), 0, 0));
        }

        if (match.Groups.TryGetValue("minutes", out var minutes) && minutes.Length > 0)
        {
            timeSpan = timeSpan.Add(new TimeSpan(0, int.Parse(minutes.Value), 0));
        }

        if (match.Groups.TryGetValue("seconds", out var seconds) && seconds.Length > 0)
        {
            timeSpan = timeSpan.Add(new TimeSpan(0, 0, int.Parse(seconds.Value)));
        }

        return timeSpan;
    }

    private static DateTime ExtractDate(string dateInput)
    {
        return DateTime.ParseExact(dateInput, "yyyy-MM-dd", CultureInfo.InvariantCulture);
    }

    private static DateTime ExtractTime(string dateInput)
    {
        var timeSpan = TimeSpan.Parse(dateInput, CultureInfo.InvariantCulture);
        return DateTime.Now.Date.Add(timeSpan);
    }

    private List<Result> GenerateErrorResponse(string description)
    {
        return
        [
            new Result
            {
                Title = "Error",
                SubTitle = description
            },
            GenerateUsageResult(),
        ];
    }

    public Result GenerateUsageResult()
    {
        return new Result
        {
            Title = "Log time",
            SubTitle = "log <issue-key> <duration ISO 8601> [date yyyy-MM-dd] [time HH:mm[:ss]]"
        };
    }
}