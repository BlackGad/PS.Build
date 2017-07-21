using System;
using System.ComponentModel;

namespace DefinitionLibrary.Property
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    [Designer("PS.Build.Adaptation")]
    public sealed class EmptyAttribute : Attribute
    {
    }
}