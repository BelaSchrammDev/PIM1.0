using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IngameScript.Tests
{
    /// <summary>
    /// Tests for StackingJob: which strategy runs on which item type in which stacking step.
    /// Uses Job.Clock to get past the cooldown between two steps.
    /// </summary>
    [TestClass]
    public class StackingJobTests
    {
        private const int Cooldown = 10;

        private DateTime _now;
        private readonly List<string> _log = new List<string>();

        [TestInitialize]
        public void SetUp()
        {
            _now = new DateTime(2026, 1, 1, 12, 0, 0);
            Program.Job.Clock = () => _now;
        }

        [TestCleanup]
        public void TearDown()
        {
            Program.Job.Clock = () => DateTime.Now;
        }

        /// <summary>Strategy stand-in: logs "name/type" for every RunJob call.</summary>
        private class Strategy : Program.Job
        {
            private readonly string _name;
            private readonly List<string> _log;
            private readonly int _runsBeforeFinished;
            private int _runs;

            public Strategy(string name, List<string> log, int runsBeforeFinished = 1)
            {
                _name = name;
                _log = log;
                _runsBeforeFinished = runsBeforeFinished;
            }

            public override void InitJob()
            {
                _runs = 0;
            }

            public override RunJobResult RunJob()
            {
                _log.Add(_name + "/" + Program.StackingJob.StackType);
                return ++_runs >= _runsBeforeFinished ? RunJobResult.Finished : RunJobResult.Continue;
            }
        }

        /// <summary>One stacking step: wait for the cooldown, then schedule until the job is done.</summary>
        private void Step(Program.StackingJob job)
        {
            _now = _now.AddSeconds(Cooldown + 1);
            var guard = 0;
            while (job.Schedule() == Program.Job.ScheduleResult.InProgress)
            {
                Assert.IsTrue(++guard < 100, "stacking step does not finish");
            }
        }

        [TestMethod]
        public void EveryStep_RunsOneStrategyOnOneType()
        {
            var job = new Program.StackingJob(Cooldown, new Strategy("MergeInContainer", _log), new Strategy("Gather", _log));

            Step(job);

            CollectionAssert.AreEqual(new[] { "MergeInContainer/Component" }, _log);
        }

        [TestMethod]
        public void AllStrategiesRunOnAllTypes_ThenItStartsOver()
        {
            var job = new Program.StackingJob(Cooldown,
                new Strategy("MergeInContainer", _log), new Strategy("Gather", _log), new Strategy("GatherInEmptiest", _log));

            for (int i = 0; i < 10; i++) Step(job);

            CollectionAssert.AreEqual(new[]
            {
                "MergeInContainer/Component", "Gather/Component", "GatherInEmptiest/Component",
                "MergeInContainer/Ore", "Gather/Ore", "GatherInEmptiest/Ore",
                "MergeInContainer/Ingot", "Gather/Ingot", "GatherInEmptiest/Ingot",
                "MergeInContainer/Component",
            }, _log);
        }

        [TestMethod]
        public void StrategyWithSeveralRuns_FinishesWithinOneStep()
        {
            var job = new Program.StackingJob(Cooldown, new Strategy("MergeInContainer", _log, runsBeforeFinished: 3), new Strategy("Gather", _log));

            Step(job);
            Step(job);

            CollectionAssert.AreEqual(new[] { "MergeInContainer/Component", "MergeInContainer/Component", "MergeInContainer/Component", "Gather/Component" }, _log);
        }

        [TestMethod]
        public void BeforeTheCooldownIsOver_NothingRuns()
        {
            var job = new Program.StackingJob(Cooldown, new Strategy("MergeInContainer", _log), new Strategy("Gather", _log));
            Step(job);

            _now = _now.AddSeconds(Cooldown - 1);
            Assert.AreEqual(Program.Job.ScheduleResult.Done, job.Schedule());

            CollectionAssert.AreEqual(new[] { "MergeInContainer/Component" }, _log);
        }

        [TestMethod]
        public void CooldownZero_StackingIsOff()
        {
            var job = new Program.StackingJob(0, new Strategy("MergeInContainer", _log));

            Assert.IsFalse(job.Active);
            Assert.AreEqual(Program.Job.ScheduleResult.Done, job.Schedule());
            Assert.AreEqual(0, _log.Count);
        }
    }
}
