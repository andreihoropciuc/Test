using System;
using Beazley.AutomationFramework.Selenium;
using log4net;

namespace Beazley.AutomationFramework.Contexts
{
    public class ImplicitWaitContext : IDisposable
    {
        protected readonly ILog Log = LogManager.GetLogger(typeof(BeazleyWebDriverTestBase));
        private readonly string _logDetails;
        public ImplicitWaitContext(TimeSpan implicitWaitTimeout, string logDetails = null)
        {
            _logDetails = logDetails;
            Globals.Driver.ChangeImplicitTimeout(implicitWaitTimeout);
            Log.Info(string.Format("Started: Overriden implicit wait to {0}. {1}", implicitWaitTimeout, logDetails ?? string.Empty));

        }

        public void Dispose()
        {
            Globals.Driver.RestoreImplicitTimeout();
            Log.Info(string.Format("Ended: Restored implicit wait to {0}. {1}", Globals.ImplicitWaitDefault, _logDetails ?? string.Empty));
        }

        public static ImplicitWaitContext Create(TimeSpan timeToWait, string logDetails = null)
        {
            return new ImplicitWaitContext(timeToWait, logDetails);
        }
    }
}
