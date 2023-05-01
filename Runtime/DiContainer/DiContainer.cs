using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Hermer29.Almasury
{
    public class DiContainer : IDiContainer
    {
        private Dictionary<Type, ContractPrefs> _instances = new Dictionary<Type, ContractPrefs>();
        internal bool _logging;

        public MetadataBuilder Add<T>()
        {
            var prefs = new ContractPrefs();
            _instances.Add(typeof(T), prefs);
            return new MetadataBuilder(prefs);
        }

        public MetadataBuilder Add<T>(T instance)
        {
            var prefs = new ContractPrefs { instance = instance };
            _instances.Add(typeof(T), prefs);
            return new MetadataBuilder(prefs);
        }

        public IEnumerator<(Type, ContractPrefs)> GetEnumerator()
        {
            return _instances.Select(x => (x.Key, x.Value))
                .GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void EnableLogging()
        {
            _logging = true;
        }
    }
}