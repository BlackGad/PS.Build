using System;
using System.ComponentModel;

namespace DefinitionLibrary.Assembly
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    [Designer("PS.Build.Adaptation")]
    public sealed class EmptyAttribute : Attribute
    {
    }
}