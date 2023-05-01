using System.Reflection;

namespace Hermer29.Almasury.Internal
{
    public static class MethodInfoExtensions
    {
        public static bool ParametersCountEquals(this MethodInfo methodInfo, int count)
        {
            return methodInfo.GetParameters().Length == count;
        }
    }
}