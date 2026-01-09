using System;

namespace CoinUpAPI.Security
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class AdminOnlyAttribute : Attribute
    {
    }
}
