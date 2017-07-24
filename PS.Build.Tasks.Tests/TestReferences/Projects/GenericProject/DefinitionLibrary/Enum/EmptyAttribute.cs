using System;
using System.ComponentModel;

namespace DefinitionLibrary.Enum
{
    [AttributeUsage(AttributeTargets.Enum, AllowMultiple = true)]
    [Designer("PS.Build.Adaptation")]
    public sealed class EmptyAttribute : Attribute
    {
    }
}