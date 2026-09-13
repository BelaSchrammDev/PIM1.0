using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IngameScript.Tests
{
    [TestClass]
    public class StringBuilderExtendedTests
    {
        private static Program.StringBuilderExtended Make(string text)
        {
            var sbx = new Program.StringBuilderExtended(64);
            sbx.SetText(text);
            return sbx;
        }

        [TestMethod]
        public void SetText_ReplacesPreviousContent()
        {
            var sbx = Make("first");
            sbx.SetText("second");

            Assert.AreEqual("second", sbx.ToString());
        }

        [TestMethod]
        public void Append_AddsToTheEnd()
        {
            var sbx = Make("ab");
            sbx.Append("cd");
            sbx.Append('e');

            Assert.AreEqual("abcde", sbx.ToString());
        }

        [TestMethod]
        public void AppendLFifNotEmpty_SkipsEmptyStrings()
        {
            var sbx = Make("");
            sbx.AppendLFifNotEmpty("");
            sbx.AppendLFifNotEmpty("line");

            Assert.AreEqual("line\n", sbx.ToString());
        }

        [TestMethod]
        public void Trim_RemovesWhitespaceOnBothEnds()
        {
            var sbx = Make("  \t padded \n ");
            sbx.Trim();

            Assert.AreEqual("padded", sbx.ToString());
        }

        [TestMethod]
        public void Trim_OnAllWhitespace_LeavesEmptyBuilder()
        {
            var sbx = Make("   ");
            sbx.Trim();

            Assert.IsTrue(sbx.IsEmpty());
        }

        [TestMethod]
        public void ToLower_LowercasesInPlace()
        {
            var sbx = Make("MiXeD");
            sbx.ToLower();

            Assert.AreEqual("mixed", sbx.ToString());
        }

        [TestMethod]
        public void Substring_WithLength_CopiesTheRequestedRange()
        {
            var source = Make("abcdef");
            var target = Make("old content");

            source.Substring(target, 2, 3);

            Assert.AreEqual("cde", target.ToString());
        }

        [TestMethod]
        public void Substring_WithoutLength_CopiesToTheEnd()
        {
            var source = Make("abcdef");
            var target = Make("old content");

            source.Substring(target, 4);

            Assert.AreEqual("ef", target.ToString());
        }

        [TestMethod]
        public void Substring_StartBeyondEnd_ClearsTargetAndCopiesNothing()
        {
            var source = Make("abc");
            var target = Make("old content");

            source.Substring(target, 99);

            Assert.AreEqual("", target.ToString());
        }

        [TestMethod]
        public void IndexOf_FindsMatchAtStartAndInTheMiddle()
        {
            Assert.AreEqual(0, Make("abc").IndexOf("abc"));
            Assert.AreEqual(2, Make("xxabc").IndexOf("abc"));
            Assert.AreEqual(6, Make("hello world").IndexOf("world"));
        }

        [TestMethod]
        public void IndexOf_ReturnsMinusOneWhenAbsent()
        {
            Assert.AreEqual(-1, Make("hello").IndexOf("xyz"));
            Assert.AreEqual(-1, Make("").IndexOf("abc"));
        }

        // --- The two tests below currently FAIL. They document real defects. ---

        [TestMethod]
        public void IndexOf_TextShorterThanNeedle_MustNotReportAMatch()
        {
            // The final check reads "sIndex < s.Length - 1" instead of "sIndex < s.Length",
            // so a needle that runs one character past the end still counts as found.
            Assert.AreEqual(-1, Make("ab").IndexOf("abc"));
        }

        [TestMethod]
        public void IndexOf_AfterPartialMatch_ResumesSearchCorrectly()
        {
            // On a mismatch the scan restarts at the current position instead of
            // backtracking, so the match starting at index 1 is never seen.
            Assert.AreEqual(1, Make("aaab").IndexOf("aab"));
        }
    }
}