using System;
using System.ComponentModel;

namespace DefinitionLibrary.Constructor
{
    [AttributeUsage(AttributeTargets.Constructor, AllowMultiple = true)]
    [Designer("PS.Build.Adaptation")]
    public sealed class EmptyAttribute : Attribute
    {
    }
}