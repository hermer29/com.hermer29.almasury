using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Hermer29.Almasury
{
    public class InjectedContainer : IEnumerable<(Type, ContractPrefs)>
    {
        private Dictionary<Type, ContractPrefs> _injected;

        public InjectedContainer(IEnumerable<(Type, ContractPrefs)> contracts)
        {
            _injected = new Dictionary<Type, ContractPrefs>();
            foreach ((Type, ContractPrefs) contract in contracts)
            {
                _injected.Add(contract.Item1, contract.Item2);
            }
            ThrowIfInvalid();
            InjectAll();
        }

        private void ThrowIfInvalid()
        {
            ValidateInitialStateMetadata();
        }

        private void ValidateInitialStateMetadata()
        {
            var foundFirst = false;
            KeyValuePair<Type, ContractPrefs> found = default;
            foreach (var injected in _injected.Where(injected => injected.Value.isFirstState))
            {
                if (foundFirst)
                {
                    throw new InvalidOperationException(
                        $"Wrong metadata found in types {injected.Key} and {found.Key}, they both defined as initial state");
                }

                found = injected;
                foundFirst = true;
            }
        }

        public IEnumerable<State> GetStates()
        {
            return _injected.Where(contract => typeof(State).IsAssignableFrom(contract.Key))
                .Select(x => x.Value.instance as State);
        }

        public State GetFirst()
        {
            if (_injected.Any(x => x.Value.isFirstState))
            {
                return (State) _injected.FirstOrDefault(x => x.Value.isFirstState).Value.instance;
            }
            return GetStates().First();
        }

        private void InjectAll()
        {
            foreach (var entry in _injected)
            {
                Inject(entry.Key, entry.Value.instance);
            }
        }

        private void Inject(Type type, object instance)
        {
            if (_injected.ContainsKey(type) && instance != null)
            {
                return;
            }
            if (instance == null)
            {
                TryActivateType(type);
            }
        }

        private void TryActivateType(Type type)
        {
            ConstructorInfo ctor = GetEligibleConstructor(type);
            foreach (ParameterInfo parameter in ctor.GetParameters())
            {
                ThrowIfCircular(type, parameter);
                Inject(parameter.ParameterType, null);
                if (_injected.ContainsKey(parameter.ParameterType) == false)
                {
                    throw new InvalidOperationException(
                        $"Dependency {parameter.ParameterType} not registered in container");
                }
            }
            object activated = Activator.CreateInstance(type, GetInstancesFromContainerForCtor(ctor));
            _injected[type].instance = activated;
        }

        private object[] GetInstancesFromContainerForCtor(MethodBase constructor)
        {
            return constructor.GetParameters()
                .Select(type => _injected[type.ParameterType].instance)
                .ToArray();
        }

        private static void ThrowIfCircular(Type type, ParameterInfo parameter)
        {
            if (IsCircular(parameter.ParameterType, type))
            {
                throw new InvalidOperationException(
                    $"Circular dependency detected. {type} defines constructor with parameter {parameter.ParameterType} and vice versa");
            }
        }

        private static bool IsCircular(Type typeA, Type typeB)
        {
            bool ctorAHasBAsParam = GetEligibleConstructor(typeA)
                .GetParameters()
                .Any(x => x.ParameterType == typeB);

            bool ctorBHasAAsParam = GetEligibleConstructor(typeB)
                .GetParameters()
                .Any(x => x.ParameterType == typeA);

            return ctorAHasBAsParam && ctorBHasAAsParam;
        }

        private static ConstructorInfo GetEligibleConstructor(Type type)
        {
            ConstructorInfo constructor = type.GetConstructors()
                .Aggregate((mostLessDeps, ctor) => 
                    ctor.GetParameters().Length < mostLessDeps.GetParameters().Length ? ctor : mostLessDeps);
            return constructor;
        }
        
        public IEnumerator<(Type, ContractPrefs)> GetEnumerator()
        {
            return _injected.Select(x => (x.Key, x.Value))
                .GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}