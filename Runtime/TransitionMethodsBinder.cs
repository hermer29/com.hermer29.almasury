using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Hermer29.Almasury.Exceptions;
using Hermer29.Almasury.Internal;
using UnityEngine;

namespace Hermer29.Almasury
{
    public class TransitionMethodsBinder
    {
        private Dictionary<Type, MethodInfo[]> _transitionMethods;
        private readonly State[] _states;
        private bool _initialized;

        public TransitionMethodsBinder(State[] states)
        {
            _states = states;
        }

        private void TryInitialize()
        {
            if (_initialized)
                return;
            _transitionMethods = new Dictionary<Type, MethodInfo[]>();
            var stateTypes = _states.Select(x => x.GetType())
                .GroupBy(x => x)
                .Select(x => x.First());
            
            foreach (Type state in stateTypes)
            {
                _transitionMethods.Add(state, GetTransitionMethods(state));
            }

            AssertThatTransitionMethodsIsValid();
        }
        
        private MethodInfo[] GetTransitionMethods(Type state)
        {
            var methods = state.GetMethods()
                .Where(x => x.GetCustomAttribute<TransitionAttribute>() != null)
                .ToArray();
            
            foreach (MethodInfo method in methods)
            {
                if (method.ReturnParameter.ParameterType != typeof(bool))
                {
                    Debug.LogError($"Invalid method ({method.Name}) defined in type {method.DeclaringType.Name}. " +
                                   $"Methods defined with TransitionAttribute must be predicates");
                }
            }

            return methods;
        }

        public bool IsTransitionRequired(object state, out Type type)
        {
            TryInitialize();
            var methods = _transitionMethods[state.GetType()];
            type = null;
            foreach (MethodInfo method in methods)
            {
                if (ExecuteTransitionMethod(state, out type, method)) 
                    return true;
            }
            return false;
        }

        private static bool ExecuteTransitionMethod(object state, out Type type, MethodInfo method)
        {
            type = null;
            var parameters = method.GetParameters();
            if (parameters.Length == 0)
            {
                if (TryInvokeTransitionMethod(state, out type, method))
                    return true;
            }
            else if (parameters[0].IsOut)
            {
                if (TryInvokeTransitionMethodAndGetTarget(state, out type, method))
                    return true;
            }

            return false;
        }

        private static bool TryInvokeTransitionMethod(object state, out Type type, MethodInfo method)
        {
            type = null;
            if ((bool)method.Invoke(state, Array.Empty<object>()))
            {
                type = method.GetCustomAttribute<TransitionAttribute>().To;
                return true;
            }
            return false;
        }

        private static bool TryInvokeTransitionMethodAndGetTarget(object state, out Type type, MethodInfo method)
        {
            type = null;
            var targetType = (object)null;
            var args = new object[] { targetType };
            if ((bool)method.Invoke(state, args))
            {
                type = (Type)args[0];
                return true;
            }
            return false;
        }

        private void AssertThatTransitionMethodsIsValid()
        {
            foreach (var stateTypeWithMethods in _transitionMethods)
            {
                ValidateStateTransitionMethods(stateTypeWithMethods);
            }
        }

        private void ValidateStateTransitionMethods(KeyValuePair<Type, MethodInfo[]> stateTypeWithMethods)
        {
            foreach (MethodInfo transitionMethod in stateTypeWithMethods.Value)
            {
                ValidateTransitionMethod(stateTypeWithMethods, transitionMethod);
            }
        }

        private void ValidateTransitionMethod(KeyValuePair<Type, MethodInfo[]> stateTypeWithMethods, MethodInfo transitionMethod)
        {
            TransitionAttribute attribute = transitionMethod.GetCustomAttribute<TransitionAttribute>();
            var stateType = stateTypeWithMethods.Key;
            
            if (HasDefinedTransitionTarget(attribute))
            {
                if (transitionMethod.ParametersCountEquals(0) == false)
                {
                    ThrowThat(transitionMethod, stateType, "Must be no parameters");
                }
                if (IsReferencedStateNotExists(attribute))
                { 
                    ThrowThat(transitionMethod, stateType, $"State machine must contain state {attribute.To}");
                }
                return;
            }

            if (transitionMethod.ParametersCountEquals(1) == false)
            {
                ThrowThat(transitionMethod, stateType, "When using transition attribute without parameter you must pass type from single out parameter");
            }

            if (CheckIsOutPositionalParameterExists(transitionMethod) == false)
            {
                ThrowThat(transitionMethod, stateType, "When using transition attribute without parameter you must pass type from single out parameter");
            }
        }

        private static void ThrowThat(MethodInfo transitionMethod, Type stateType, string reason)
        {
            throw new TransitionMethodException(
                stateType: stateType,
                method: transitionMethod,
                message: reason);
        }

        private bool IsReferencedStateNotExists(TransitionAttribute attribute)
        {
            return _states.Any(x => attribute.To == x.GetType()) == false;
        }

        private static bool HasDefinedTransitionTarget(TransitionAttribute attribute)
        {
            return attribute.To != null;
        }

        private static bool CheckIsOutPositionalParameterExists(MethodInfo transitionMethod)
        {
            ParameterInfo parameter = transitionMethod.GetParameters().First();
            return parameter.IsOut;
        }
    }
}