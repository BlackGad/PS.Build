using System;
using System.ComponentModel;

namespace DefinitionLibrary.Module
{
    [AttributeUsage(AttributeTargets.Module, AllowMultiple = true)]
    [Designer("PS.Build.Adaptation")]
    public sealed class EmptyAttribute : Attribute
    {
    }
}