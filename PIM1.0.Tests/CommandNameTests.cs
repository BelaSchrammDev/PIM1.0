using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IngameScript.Tests
{
    /// <summary>
    /// Tests for the command name of a run argument (NetWorkingParser.ParseCommandName)
    /// and for the values of a networking message body (NetWorkingParser.GetValue).
    /// </summary>
    [TestClass]
    public class CommandNameTests
    {
        [DataTestMethod]
        [DataRow("flushrefinerys_all", "flushrefinerys_all")]
        [DataRow("flushrefinerys_all()", "flushrefinerys_all")]
        [DataRow("FlushRefinerys_All", "flushrefinerys_all")]
        [DataRow("  flushrefinerys_all  ", "flushrefinerys_all")]
        [DataRow("say_hello(emitter_id=1233445565)", "say_hello")]
        [DataRow("Say_Hello (emitter_id=1;version=2)", "say_hello")]
        [DataRow("", "")]
        [DataRow("(x)", "")]
        public void ParseCommandName_IsTheLowerCaseTextBeforeTheBracket(string commandLine, string expected)
        {
            Assert.AreEqual(expected, Program.NetWorkingParser.ParseCommandName(commandLine));
        }

        [DataTestMethod]
        [DataRow("type=PIM;version=PIM v1.1", "type", "PIM")]
        [DataRow("type=PIM;version=PIM v1.1", "version", "PIM v1.1")]
        [DataRow("a=x=y", "a", "x=y")]
        [DataRow("a=", "a", "")]
        [DataRow("type=PIM", "version", "")]
        [DataRow("typex=PIM;type=SMS", "type", "SMS")]
        [DataRow("", "type", "")]
        [DataRow("=x;type", "type", "")]
        public void GetValue_IsTheValueOfTheKeyOrEmpty(string body, string key, string expected)
        {
            Assert.AreEqual(expected, Program.NetWorkingParser.GetValue(body, key));
        }
    }
}
