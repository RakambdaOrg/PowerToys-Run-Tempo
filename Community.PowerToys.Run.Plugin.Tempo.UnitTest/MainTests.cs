using Microsoft.VisualStudio.TestTools.UnitTesting;
using PowerToys_Run_Tempo;
using Wox.Plugin;

namespace PowerToys_Run_Tempo_UnitTest;

[TestClass]
public class MainTests
{
    private Main _subject = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        DotNetEnv.Env.TraversePath().Load();
        TestUtils.MockLogs();
        
        _subject = new Main(new PowerToys_Run_Tempo.Settings
        {
            AccountId = System.Environment.GetEnvironmentVariable("JIRA_ACCOUNT_ID") ?? "",
            JiraEndpoint = System.Environment.GetEnvironmentVariable("JIRA_ENDPOINT") ?? "",
            JiraUsername = System.Environment.GetEnvironmentVariable("JIRA_USERNAME") ?? "",
            JiraToken = System.Environment.GetEnvironmentVariable("JIRA_TOKEN") ?? "",
            TempoToken = System.Environment.GetEnvironmentVariable("TEMPO_TOKEN") ?? ""
        });
        _subject.InitSelf();
    }

    [TestMethod]
    public void ItGivesResult()
    {
        var results = _subject.Query(new Query("log"));       
        Assert.AreEqual(1, results.Count);
    }
}