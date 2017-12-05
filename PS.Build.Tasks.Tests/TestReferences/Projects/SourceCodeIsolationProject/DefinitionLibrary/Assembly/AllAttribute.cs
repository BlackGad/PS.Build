using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using Microsoft.CodeAnalysis;
using PS.Build.Services;

namespace DefinitionLibrary.Assembly
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    [Designer("PS.Build.Adaptation")]
    public sealed class AllAttribute : Attribute
    {
        #region Constructors

        #endregion

        #region Members

        void PostBuild(IServiceProvider provider)
        {
            var logger = (ILogger)provider.GetService(typeof(ILogger));
            var syntaxTree = (SyntaxTree)provider.GetService(typeof(SyntaxTree));
            var message = string.Join(",", nameof(PostBuild), Path.GetFileNameWithoutExtension(syntaxTree.FilePath));
            logger.Info(message);
        }

        void PreBuild(IServiceProvider provider)
        {
            var logger = (ILogger)provider.GetService(typeof(ILogger));
            var syntaxTree = (SyntaxTree)provider.GetService(typeof(SyntaxTree));
            var message = string.Join(",", nameof(PreBuild), Path.GetFileNameWithoutExtension(syntaxTree.FilePath));
            logger.Info(message);
        }

        #endregion
    }
}