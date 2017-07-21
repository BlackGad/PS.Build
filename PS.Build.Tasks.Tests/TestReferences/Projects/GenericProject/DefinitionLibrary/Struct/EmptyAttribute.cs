using System;
using System.ComponentModel;

namespace DefinitionLibrary.Struct
{
    [AttributeUsage(AttributeTargets.Struct, AllowMultiple = true)]
    [Designer("PS.Build.Adaptation")]
    public sealed class EmptyAttribute : Attribute
    {
    }
}