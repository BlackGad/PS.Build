using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DefinitionLibrary.Method
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    [Designer("PS.Build.Adaptation")]
    public sealed class EmptyAttribute : BaseAttribute
    {
        #region Constructors

        public EmptyAttribute([CallerLineNumber] int position = default(int), [CallerFilePath] string file = null) : base(position, file)
        {
        }

        #endregion
    }
}