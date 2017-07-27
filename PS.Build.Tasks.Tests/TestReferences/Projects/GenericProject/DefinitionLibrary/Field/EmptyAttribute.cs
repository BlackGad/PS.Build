﻿using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DefinitionLibrary.Field
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
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