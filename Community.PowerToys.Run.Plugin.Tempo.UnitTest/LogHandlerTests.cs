using System;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PowerToys_Run_Tempo.handler;
using PowerToys_Run_Tempo.jira.api;
using PowerToys_Run_Tempo.jira.api.model;
using PowerToys_Run_Tempo.tempo.api;
using Wox.Plugin;

namespace PowerToys_Run_Tempo_UnitTest;

[TestClass]
public class LogHandlerTests
{
    private LogHandler _subject = null!;
    private Mock<JiraApi> _jiraApi = null!;
    private Mock<TempoApi> _tempoApi = null!;
    private string _issueKey = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        DotNetEnv.Env.TraversePath().Load();
        TestUtils.MockLogs();
        
        _issueKey = Environment.GetEnvironmentVariable("ISSUE_KEY") ?? "";

        _jiraApi = new Mock<JiraApi>("", "", "");
        _tempoApi = new Mock<TempoApi>("");

        _subject = new LogHandler(
            System.Environment.GetEnvironmentVariable("JIRA_ACCOUNT_ID") ?? "",
            _jiraApi.Object,
            _tempoApi.Object
        );
    }

    [DataRow(0)]
    [DataRow(1)]
    [DataRow(2)]
    [DataTestMethod]
    public void ShouldDoNothingWhenNotTheCorrectNumberOfParameters(int paramsCount)
    {
        var query = new Query(string.Join(" ", Enumerable.Range(1, paramsCount).Select(_ => "random")));
        var results = _subject.HandleQuery(query);
        Assert.AreEqual(1, results.Count);
        Assert.AreEqual("Log time", results[0].Title);
        Assert.AreEqual("log <issue-key> <duration ISO 8601> [date yyyy-MM-dd] [time HH:mm[:ss]]", results[0].SubTitle);
    }

    [TestMethod]
    public void ShouldGetIssueInfoAndCreateResult()
    {
        const string issueSummary = "Issue summary";
        _jiraApi.Setup(api => api.GetIssueInfo(_issueKey)).Returns(new IssueInfoResponse("123", _issueKey, new Fields(issueSummary)));

        var query = new Query($"log {_issueKey} 2H");
        var results = _subject.HandleQuery(query);

        Assert.AreEqual(1, results.Count);
        StringAssert.Matches(results[0].Title, new Regex($@"Log time on {_issueKey} at \d{{4}}-\d{{2}}-\d{{2}} \d+:\d+:\d+ for 2H"));
        Assert.AreEqual(issueSummary, results[0].SubTitle);

        _tempoApi.VerifyAll();
    }

    [TestMethod]
    public void ShouldGetIssueInfoAndCreateResultWithMoreParams()
    {
        const string issueSummary = "Issue summary";
        _jiraApi.Setup(api => api.GetIssueInfo(_issueKey)).Returns(new IssueInfoResponse("123", _issueKey, new Fields(issueSummary)));

        var query = new Query($"log {_issueKey} 2H 2024-12-08 02:30");
        var results = _subject.HandleQuery(query);

        Assert.AreEqual(1, results.Count);
        Assert.AreEqual($"Log time on {_issueKey} at 2024-12-08 02:30:00 for 2H", results[0].Title);
        Assert.AreEqual(issueSummary, results[0].SubTitle);

        _tempoApi.VerifyAll();
    }

    [TestMethod]
    public void ShouldDisplayErrorOnUnknownIssue()
    {
        _jiraApi.Setup(api => api.GetIssueInfo(_issueKey)).Returns((IssueInfoResponse)null);

        var query = new Query($"log {_issueKey} 2H");
        var results = _subject.HandleQuery(query);

        Assert.AreEqual(2, results.Count);
        Assert.AreEqual("Error", results[0].Title);
        Assert.AreEqual("Failed to get issue ID from Jira API", results[0].SubTitle);

        _tempoApi.VerifyAll();
    }

    [TestMethod]
    public void ShouldDisplayErrorOnUnknownDuration()
    {
        const string issueSummary = "Issue summary";
        _jiraApi.Setup(api => api.GetIssueInfo(_issueKey)).Returns(new IssueInfoResponse("123", _issueKey, new Fields(issueSummary)));

        var query = new Query($"log {_issueKey} 2U");
        var results = _subject.HandleQuery(query);

        Assert.AreEqual(2, results.Count);
        Assert.AreEqual("Error", results[0].Title);
        Assert.AreEqual("Failed to parse duration", results[0].SubTitle);

        _tempoApi.VerifyAll();
    }
}