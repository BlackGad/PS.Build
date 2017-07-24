using System;
using System.ComponentModel;

namespace DefinitionLibrary.GenericParameter
{
    [AttributeUsage(AttributeTargets.GenericParameter, AllowMultiple = true)]
    [Designer("PS.Build.Adaptation")]
    public sealed class EmptyAttribute : Attribute
    {
    }
}