using System.Net;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PowerToys_Run_Tempo.api.tempo.model;
using PowerToys_Run_Tempo.tempo.api;

namespace PowerToys_Run_Tempo_UnitTest;

[TestClass]
public class TempoApiTests
{
    private TempoApi _subject = null!;
    private string _accountId = null!;
    private long? _issueId;

    [TestInitialize]
    public void TestInitialize()
    {
        DotNetEnv.Env.TraversePath().Load();
        TestUtils.MockLogs();
        
        _subject = new TempoApi(System.Environment.GetEnvironmentVariable("TEMPO_TOKEN") ?? "");

        _accountId = System.Environment.GetEnvironmentVariable("JIRA_ACCOUNT_ID") ?? "";
        _issueId = long.Parse(System.Environment.GetEnvironmentVariable("ISSUE_ID") ?? "0");
    }

    [TestMethod]
    public void ShouldAddWorklog()
    {
        var schema = new AddWorklogSchema(
            _accountId,
            _issueId ?? 0,
            "2024-12-09",
            "12:00:00",
            1800
        );
        var result = _subject.AddWorklog(schema);
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.tempoWorklogId);

        _subject.DeleteWorklog(result.tempoWorklogId.ToString());
    }

    [TestMethod]
    public void ShouldThrowWhenWorkingTooMuchInADay()
    {
        var schema = new AddWorklogSchema(
            _accountId,
            _issueId ?? 0,
            "2024-12-09",
            "12:00:00",
            180000000
        );
        var thrown = Assert.ThrowsException<TempoApiException>(() => _subject.AddWorklog(schema));
        Assert.AreEqual(HttpStatusCode.BadRequest, thrown.StatusCode);
        Assert.IsNotNull(thrown.ErrorResponse);
        Assert.AreEqual(1, thrown.ErrorResponse.errors.Length);
        StringAssert.Matches(thrown.ErrorResponse.errors[0].message, new Regex(@"\d+h exceeds daily limit of 8h"));
    }

    [TestMethod]
    public void ShouldThrowOnUnknownIssue()
    {
        var schema = new AddWorklogSchema(
            _accountId,
            0,
            "2024-12-09",
            "12:00:00",
            1800
        );
        var thrown = Assert.ThrowsException<TempoApiException>(() => _subject.AddWorklog(schema));
        Assert.AreEqual(HttpStatusCode.BadRequest, thrown.StatusCode);
        Assert.IsNotNull(thrown.ErrorResponse);
        Assert.AreEqual(1, thrown.ErrorResponse.errors.Length);
        Assert.AreEqual("Invalid issue id", thrown.ErrorResponse.errors[0].message);
    }
    
    [TestMethod]
    public void ShouldGetGlobalConfiguration()
    {
        var result = _subject.GetGlobalConfiguration();
        Assert.IsNotNull(result);
        Assert.IsTrue(result.maxHoursPerDayPerUser > 0);
    }
}