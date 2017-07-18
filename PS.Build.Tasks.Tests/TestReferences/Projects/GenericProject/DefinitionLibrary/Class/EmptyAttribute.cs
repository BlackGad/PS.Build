using System;
using System.ComponentModel;

namespace DefinitionLibrary.Class
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    [Designer("PS.Build.Adaptation")]
    public sealed class EmptyAttribute : Attribute
    {
    }
}