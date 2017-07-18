using System;
using System.ComponentModel;

namespace DefinitionLibrary.Method
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    [Designer("PS.Build.Adaptation")]
    public sealed class EmptyAttribute : Attribute
    {
    }
}