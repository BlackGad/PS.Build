using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DefinitionLibrary.Parameter
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = true)]
    [Designer("PS.Build.Adaptation")]
    public sealed class EmptyAttribute : BaseAttribute
    {
        #region Constructors

        public EmptyAttribute([CallerFilePath] string file = null) : base(file)
        {
        }

        #endregion
    }
}