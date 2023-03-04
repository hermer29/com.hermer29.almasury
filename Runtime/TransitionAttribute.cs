using System;

namespace StateMachines.Runtime
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class TransitionAttribute : Attribute
    {
        public readonly Type To;

        public TransitionAttribute(Type to) => To = to;

        public TransitionAttribute() => To = null;
    }
}