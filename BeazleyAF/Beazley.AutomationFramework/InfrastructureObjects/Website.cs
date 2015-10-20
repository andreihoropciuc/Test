using System;
using System.Collections.Generic;
using System.Linq;

namespace Beazley.AutomationFramework.InfrastructureObjects
{
    public class Website
    {
        private static Dictionary<string, string> _entryPoints;

        public static void Initialize(IDictionary<string, string> entryPointsConfig)
        {
            if (_entryPoints == null)
                _entryPoints = entryPointsConfig.ToDictionary(x => x.Key, x => x.Value);
        }

        public static T GoTo<T>()
            where T : BasePage
        {
            if (_entryPoints == null)
                throw new Exception("Entry Points collection not initialised");

            if (_entryPoints.ContainsKey(typeof(T).Name))
                return (T)Activator.CreateInstance(typeof(T), Globals.Driver, null, _entryPoints[typeof(T).Name], null);

            throw new Exception(string.Format("Requested Page ({0}) is not defined as Entry Point. Please Check Entry Points config.", typeof(T).Name));
        }
    }
}
