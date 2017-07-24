using System;
using System.ComponentModel;

namespace DefinitionLibrary.ReturnValue
{
    [AttributeUsage(AttributeTargets.ReturnValue, AllowMultiple = true)]
    [Designer("PS.Build.Adaptation")]
    public sealed class EmptyAttribute : Attribute
    {
    }
}