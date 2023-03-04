using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Assertions;

namespace StateMachines.Runtime
{
    public class StateMachine
    {
        private readonly State[] _states;
        private State _current;
        private Dictionary<Type, MethodInfo[]> _transitionMethods;

        public StateMachine(int firstStateIndex, params State[] states)
        {
            _states = states;
            _current = states[firstStateIndex];
        }
        
        public StateMachine(params State[] states) : this(0, states)
        {
        }

        public void OnEnable()
        {
            foreach (var state in _states)
            {
                state.Initialize();
            }
            AssignTransitionMethods();
            _current.Enter();
        }

        private void AssignTransitionMethods()
        {
            if (_transitionMethods == null)
            {
                _transitionMethods = new Dictionary<Type, MethodInfo[]>();
                foreach (var state in _states.Select(x => x.GetType()).GroupBy(x => x))
                {
                    _transitionMethods.Add(state.Key, GetTransitionMethods(state.Key));
                }

                AssertThatTransitionMethodsIsValid();
            }
        }

        private void AssertThatTransitionMethodsIsValid()
        {
            foreach (var stateTypeWithMethods in _transitionMethods)
            {
                foreach (var transitionMethod in stateTypeWithMethods.Value)
                {
                    var stateType = transitionMethod.GetCustomAttribute<TransitionAttribute>().To;
                    if(_states.All(x => stateType != x.GetType()))
                    {
                        throw new InvalidOperationException(
                            $"State {stateTypeWithMethods.Key} define transition method {transitionMethod.Name} with transition to {stateType}, which is not presented in state machine!");
                    }
                }
            }
        }

        private MethodInfo[] GetTransitionMethods(Type state)
        {
            var methods = state.GetMethods()
                .Where(x => x.GetCustomAttribute<TransitionAttribute>() != null)
                .ToArray();
            
            foreach (var method in methods)
            {
                if (method.ReturnParameter.ParameterType != typeof(bool))
                {
                    Debug.LogError($"Invalid method ({method.Name}) defined in type {method.DeclaringType.Name}. " +
                                   $"Methods defined with TransitionAttribute must be predicates");
                }
            }

            return methods;
        }

        public void OnDisable()
        {
            foreach (var state in _states)
            {
                state.OnDisable();
            }
            _transitionMethods.Clear();
            _transitionMethods = null;
        }

        public void Update(float deltaTime)
        {
            var transitionTarget = GetTransitionTarget(_current);
            if (transitionTarget != null)
            {
                _current.Exit();
                _current = _states.First(x => x.GetType() == transitionTarget);
                _current.Enter();
            }
            _current.Update(deltaTime);
        }

        private Type GetTransitionTarget(object state)
        {
            var methods = _transitionMethods[state.GetType()];
            foreach (var method in methods)
            {
                if ((bool) method.Invoke(state, Array.Empty<object>()))
                {
                    return method.GetCustomAttribute<TransitionAttribute>().To;
                }
            }

            return null;
        }
    }
}