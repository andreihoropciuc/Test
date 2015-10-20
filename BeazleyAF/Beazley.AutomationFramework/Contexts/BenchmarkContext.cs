using System;
using System.Diagnostics;
using log4net;

namespace Beazley.AutomationFramework.Contexts
{
    public class BenchmarkContext : IDisposable
    {
        protected readonly ILog Log = LogManager.GetLogger(typeof(BenchmarkContext));

        private readonly Stopwatch _stopwatch;
        private readonly string _description;
        private readonly bool _enabled;
        public BenchmarkContext(string description, bool enabled = true)
        {
            _enabled = enabled;

            if (!_enabled)
                return;

            _description = description;
            _stopwatch = new Stopwatch();

            Log.Info(string.Format("Starting {0}", description));

            _stopwatch.Start();
        }

        public TimeSpan Elapsed
        {
            get { return _enabled ? _stopwatch.Elapsed : TimeSpan.MaxValue; }
        }

        public void Dispose()
        {
            if (!_enabled)
                return;

            _stopwatch.Stop();
            Log.Info(string.Format("Finished {0}. Elapsed = {1}", _description, _stopwatch.Elapsed));
        }
    }
}
