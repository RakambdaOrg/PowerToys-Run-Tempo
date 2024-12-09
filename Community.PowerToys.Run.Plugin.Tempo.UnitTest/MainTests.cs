using System.IO;
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
        _subject = new Main();
        _subject.InitPluginPath(Path.Join("."));
    }

    [TestMethod]
    public void Query_should_be_empty()
    {
        var results = _subject.Query(new Query(""));
        Assert.AreEqual(0, results.Count);
    }
}