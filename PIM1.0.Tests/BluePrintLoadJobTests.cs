using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IngameScript.Tests
{
    /// <summary>
    /// The blueprints are loaded by a job at script start (one step per data entry).
    /// The results are covered by BluePrintGoldenTests; these tests check the job behaviour.
    /// </summary>
    [TestClass]
    public class BluePrintLoadJobTests
    {
        static Dictionary<string, bool> Mods(bool enabled) =>
            Program.BluePrintMods.Where(m => m != Program.Strings.M_Vanilla).ToDictionary(m => m, m => enabled);

        static int EntryCount(System.Func<string, bool> enabled) =>
            Program.BluePrintMods.Select((m, i) => enabled(m) ? Program.BluePrintData[i].Split(',').Length : 0).Sum();

        [TestMethod]
        public void LoadJob_WorksIncrementally_OneEntryPerStep()
        {
            var assembler = new Dictionary<string, Program.AssemblerBluePrint>();
            var refinery = new List<Program.RefineryBlueprint>();
            var job = new Program.BluePrintLoadJob(new Program.BluePrintSet(Mods(false), assembler, refinery));

            int steps = 0;
            while (job.Schedule() == Program.Job.ScheduleResult.InProgress)
            {
                steps++;
                if (steps == 5) Assert.IsTrue(assembler.Count > 0 && assembler.Count < 5, "blueprints are added step by step");
            }

            var vanillaEntries = EntryCount(m => m == Program.Strings.M_Vanilla);
            Assert.IsTrue(steps >= vanillaEntries, $"expected at least {vanillaEntries} steps, got {steps}");
            Assert.IsTrue(steps < vanillaEntries + 3 * Program.BluePrintMods.Length, "disabled mods cost only a few steps");
            Assert.IsTrue(refinery.Count > 0);
        }

        [TestMethod]
        public void LoadJob_RunsOnlyOnce()
        {
            var assembler = new Dictionary<string, Program.AssemblerBluePrint>();
            var refinery = new List<Program.RefineryBlueprint>();
            var job = new Program.BluePrintLoadJob(new Program.BluePrintSet(Mods(true), assembler, refinery));

            while (job.Schedule() == Program.Job.ScheduleResult.InProgress) { }
            Assert.IsFalse(job.Active, "the job deactivates itself after loading");

            var count = assembler.Count;
            for (int i = 0; i < 100; i++)
                Assert.AreEqual(Program.Job.ScheduleResult.Done, job.Schedule());
            Assert.AreEqual(count, assembler.Count);
        }

        [TestMethod]
        public void LoadJob_RefineryListIsSortedWhenDone()
        {
            var refinery = new List<Program.RefineryBlueprint>();
            Program.DefineBluePrints(Mods(true), new Dictionary<string, Program.AssemblerBluePrint>(), refinery);

            var names = refinery.Select(r => r.InputIDName).ToList();
            CollectionAssert.AreEqual(names.OrderBy(n => n).ToList(), names);
        }
    }
}
