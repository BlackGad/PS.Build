using System;
using System.ComponentModel;

namespace DefinitionLibrary.Field
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    [Designer("PS.Build.Adaptation")]
    public sealed class EmptyAttribute : Attribute
    {
    }
}