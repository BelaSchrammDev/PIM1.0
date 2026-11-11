using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IngameScript.Tests
{
    /// <summary>
    /// Tests for the command name of a run argument (NetWorkingParser.ParseCommandName)
    /// and the dispatcher that switches on it.
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
        [DataRow("say_hello")]
        [DataRow("say_hello(emitter_id=1233445565)")]
        [DataRow("SAY_HELLO()")]
        public void Dispatch_KnownCommand_IsFoundWithAndWithoutBrackets(string commandLine)
        {
            Assert.IsTrue(Program.NetWorkingDispatcher.Instance.Dispatch(commandLine));
        }

        [DataTestMethod]
        [DataRow("blabla")]
        [DataRow("blabla(x=1)")]
        [DataRow("say_hello_again")]
        [DataRow("()")]
        public void Dispatch_UnknownCommand_IsNotFound(string commandLine)
        {
            Assert.IsFalse(Program.NetWorkingDispatcher.Instance.Dispatch(commandLine));
        }
    }
}
