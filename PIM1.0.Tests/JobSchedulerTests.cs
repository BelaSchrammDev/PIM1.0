using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IngameScript.Tests
{
    /// <summary>
    /// Tests for Job.Schedule, the Init/Running/Cooling state machine every
    /// job in the loop goes through. Uses Job.Clock instead of Thread.Sleep
    /// to control cooldown timing deterministically.
    /// </summary>
    [TestClass]
    public class JobSchedulerTests
    {
        private DateTime _now;

        [TestInitialize]
        public void SetUp()
        {
            _now = new DateTime(2026, 1, 1, 12, 0, 0);
            Program.Job.Clock = () => _now;
        }

        [TestCleanup]
        public void TearDown()
        {
            // Never leak the test clock into other tests or real jobs.
            Program.Job.Clock = () => DateTime.Now;
        }

        private class CountingTestJob : Program.Job
        {
            public int InitCount;
            public int RunCount;
            public int RunsBeforeFinished;

            public CountingTestJob(int cooldownSeconds, int runsBeforeFinished) : base(cooldownSeconds)
            {
                RunsBeforeFinished = runsBeforeFinished;
            }

            public override void InitJob()
            {
                InitCount++;
            }

            public override RunJobResult RunJob()
            {
                RunCount++;
                return RunCount >= RunsBeforeFinished ? RunJobResult.Finished : RunJobResult.Continue;
            }
        }

        [TestMethod]
        public void Schedule_FirstCall_RunsInitAndReportsInProgress()
        {
            var job = new CountingTestJob(cooldownSeconds: 0, runsBeforeFinished: 1);

            var result = job.Schedule();

            Assert.AreEqual(Program.Job.ScheduleResult.InProgress, result);
            Assert.AreEqual(1, job.InitCount);
            Assert.AreEqual(0, job.RunCount);
        }

        [TestMethod]
        public void Schedule_WhileRunJobReturnsContinue_KeepsReportingInProgress()
        {
            var job = new CountingTestJob(cooldownSeconds: 0, runsBeforeFinished: 3);
            job.Schedule(); // Init -> Running

            var firstRun = job.Schedule();  // RunJob #1 -> Continue
            var secondRun = job.Schedule(); // RunJob #2 -> Continue

            Assert.AreEqual(Program.Job.ScheduleResult.InProgress, firstRun);
            Assert.AreEqual(Program.Job.ScheduleResult.InProgress, secondRun);
            Assert.AreEqual(2, job.RunCount);
        }

        [TestMethod]
        public void Schedule_WithoutCooldown_ReturnsToInitOnceClockAdvances()
        {
            var job = new CountingTestJob(cooldownSeconds: 0, runsBeforeFinished: 1);
            job.Schedule(); // Init -> Running
            job.Schedule(); // RunJob -> Finished, cooldown (0s) starts at _now

            // Schedule() uses a strict "elapsed > cooldown" comparison, so
            // even a 0s cooldown is not yet "elapsed" at the exact same
            // instant it started - this only matters with a frozen test
            // clock, since DateTime.Now always moves on in production.
            var sameInstant = job.Schedule();
            Assert.AreEqual(Program.Job.ScheduleResult.Done, sameInstant);

            _now = _now.AddTicks(1);

            // This call only flips Cooling -> Init; InitJob() itself runs on
            // the *next* call, not this one - the transition and the actual
            // re-init are two separate Schedule() calls.
            var backToInit = job.Schedule();
            Assert.AreEqual(Program.Job.ScheduleResult.InProgress, backToInit);
            Assert.AreEqual(1, job.InitCount);

            var initRunsAgain = job.Schedule();
            Assert.AreEqual(Program.Job.ScheduleResult.InProgress, initRunsAgain);
            Assert.AreEqual(2, job.InitCount);
        }

        [TestMethod]
        public void Schedule_DuringCooldown_ReturnsDoneUntilCooldownElapses()
        {
            var job = new CountingTestJob(cooldownSeconds: 10, runsBeforeFinished: 1);
            job.Schedule(); // Init -> Running
            job.Schedule(); // RunJob -> Finished, cooldown starts at _now

            var stillCooling = job.Schedule();
            Assert.AreEqual(Program.Job.ScheduleResult.Done, stillCooling);

            _now = _now.AddSeconds(5);
            var stillCoolingHalfway = job.Schedule();
            Assert.AreEqual(Program.Job.ScheduleResult.Done, stillCoolingHalfway);

            _now = _now.AddSeconds(6); // total 11s > 10s cooldown

            // Flips Cooling -> Init; InitJob() has not run yet at this point.
            var cooledDown = job.Schedule();
            Assert.AreEqual(Program.Job.ScheduleResult.InProgress, cooledDown);
            Assert.AreEqual(1, job.InitCount);

            var initRunsAgain = job.Schedule();
            Assert.AreEqual(Program.Job.ScheduleResult.InProgress, initRunsAgain);
            Assert.AreEqual(2, job.InitCount);
        }

        [TestMethod]
        public void Schedule_WhenInactive_ReturnsDoneWithoutRunningInit()
        {
            var job = new CountingTestJob(cooldownSeconds: 0, runsBeforeFinished: 1)
            {
                Active = false
            };

            var result = job.Schedule();

            Assert.AreEqual(Program.Job.ScheduleResult.Done, result);
            Assert.AreEqual(0, job.InitCount);
        }
    }
}