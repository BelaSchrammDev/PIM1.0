namespace IngameScript
{
    partial class Program
    {
        public class SequentialJob : Job
        {
            private readonly Job[] _jobs;
            private int? _currentJobIndex = null;
            public SequentialJob(params Job[] jobs)
            {
                _jobs = jobs;
            }

            public bool NextJob()
            {
                if(_currentJobIndex.HasValue)
                {
                    _currentJobIndex++;
                    if (_currentJobIndex >= _jobs.Length)
                    {
                        _currentJobIndex = null;
                        return false;
                    }
                }
                else if (_jobs.Length == 0)
                {
                    return false;
                }
                else
                {
                    _currentJobIndex = 0;
                }

                return true;
            }

            public override void InitJob()
            {
                NextJob();
            }

            public override RunJobResult RunJob()
            {
                if(!_currentJobIndex.HasValue)
                {
                    return RunJobResult.Finished;
                }

                var result = _jobs[_currentJobIndex.Value].Schedule();

                if (result == ScheduleResult.Done && !NextJob())
                {
                    return RunJobResult.Finished;
                }

                return RunJobResult.Continue;
            }
        }
    }
}
