using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PowerToys_Run_Tempo.jira.api;

namespace PowerToys_Run_Tempo_UnitTest;

[TestClass]
public class JiraApiTests
{
    private JiraApi _subject = null!;
    private string _issueKey = null!;
    private string _issueSummary = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        DotNetEnv.Env.TraversePath().Load();
        TestUtils.MockLogs();
        
        _subject = new JiraApi(
            System.Environment.GetEnvironmentVariable("JIRA_ENDPOINT") ?? "",
            System.Environment.GetEnvironmentVariable("JIRA_USERNAME") ?? "",
            System.Environment.GetEnvironmentVariable("JIRA_TOKEN") ?? ""
        );

        _issueKey = System.Environment.GetEnvironmentVariable("ISSUE_KEY") ?? "";
        _issueSummary = System.Environment.GetEnvironmentVariable("ISSUE_SUMMARY") ?? "";
    }

    [TestMethod]
    public void Should_Get_Issue_Info()
    {
        var result = _subject.GetIssueInfo(_issueKey);
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.id);
        Assert.AreEqual(_issueKey, result.key);
        Assert.IsNotNull(result.fields);
        Assert.IsNotNull(_issueSummary, result.fields.summary);
    }

    [TestMethod]
    public void Should_Throw_On_Unknown_Issue()
    {
        var thrown = Assert.ThrowsException<JiraApiException>(() => _subject.GetIssueInfo("ABC-123"));
        Assert.AreEqual(HttpStatusCode.NotFound, thrown.StatusCode);
    }
}