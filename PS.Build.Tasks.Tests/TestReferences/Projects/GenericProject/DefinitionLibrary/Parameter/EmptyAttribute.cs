using System;
using System.ComponentModel;

namespace DefinitionLibrary.Parameter
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = true)]
    [Designer("PS.Build.Adaptation")]
    public sealed class EmptyAttribute : Attribute
    {
    }
}