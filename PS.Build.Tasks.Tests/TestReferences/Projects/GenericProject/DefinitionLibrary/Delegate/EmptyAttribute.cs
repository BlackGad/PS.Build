using System;
using System.ComponentModel;

namespace DefinitionLibrary.Delegate
{
    [AttributeUsage(AttributeTargets.Delegate, AllowMultiple = true)]
    [Designer("PS.Build.Adaptation")]
    public sealed class EmptyAttribute : Attribute
    {
    }
}