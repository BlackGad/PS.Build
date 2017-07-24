using System;
using System.ComponentModel;

namespace DefinitionLibrary.Interface
{
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = true)]
    [Designer("PS.Build.Adaptation")]
    public sealed class EmptyAttribute : Attribute
    {
    }
}