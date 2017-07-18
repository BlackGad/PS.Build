using System;
using System.ComponentModel;

namespace DefinitionLibrary.Event
{
    [AttributeUsage(AttributeTargets.Event, AllowMultiple = true)]
    [Designer("PS.Build.Adaptation")]
    public sealed class EmptyAttribute : Attribute
    {
    }
}