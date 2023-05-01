using System;
using System.Reflection;

namespace Hermer29.Almasury.Exceptions
{
    public class TransitionMethodException : AlmasuryException
    {
        public TransitionMethodException(Type stateType, MethodInfo method, string message)
            : base($"At state type {stateType}, at method {method.Name}: {message}")
        {
        }
    }
}