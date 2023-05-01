using System;

namespace Hermer29.Almasury.Exceptions
{
    public class AlmasuryException : Exception 
    {
        protected AlmasuryException(string message) : base($"[Almasury] {message}")
        {
        }
    }
}