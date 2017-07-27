﻿using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DefinitionLibrary.Class
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
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