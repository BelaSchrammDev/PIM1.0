using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IngameScript.Tests
{
    [TestClass]
    public class RunningSignTests
    {
        /// <summary>The original implementation (StringBuilderExtended based), kept as reference.</summary>
        class OldRunningSign
        {
            int r = 0, rc = 1;
            const int mr = 7;

            public string Next()
            {
                var sb = new StringBuilder("|");
                r += rc;
                if (r < 0) { r = 1; rc = 1; }
                else if (r > mr) { r = mr - 1; rc = -1; }
                for (int i = 0; i <= mr; i++) sb.Append(i == r ? (rc < 0 ? '<' : '>') : ' ');
                return sb.Append("| ").ToString();
            }
        }

        [TestMethod]
        public void RunningSign_SameSequenceAsBefore()
        {
            var sign = new Program.RunningSign();
            var old = new OldRunningSign();
            for (int i = 0; i < 50; i++) Assert.AreEqual(old.Next(), sign.Next(), "call " + (i + 1));
        }

        [TestMethod]
        public void RunningSign_BouncesBetweenTheBars()
        {
            var sign = new Program.RunningSign();
            Assert.AreEqual("| >      | ", sign.Next());
            for (int i = 0; i < 5; i++) sign.Next();
            Assert.AreEqual("|       >| ", sign.Next());
            Assert.AreEqual("|      < | ", sign.Next());
        }
    }
}
