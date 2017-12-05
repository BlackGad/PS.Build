using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PS.Build.Extensions;

namespace PS.Build.Tasks
{
    class SuspiciousAttributeSyntax
    {
        #region Static members

        public static bool operator ==(SuspiciousAttributeSyntax left, SuspiciousAttributeSyntax right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(SuspiciousAttributeSyntax left, SuspiciousAttributeSyntax right)
        {
            return !Equals(left, right);
        }

        #endregion

        #region Constructors

        public SuspiciousAttributeSyntax(AttributeSyntax syntax, IEnumerable<Type> possibleTypes)
        {
            if (syntax == null) throw new ArgumentNullException(nameof(syntax));
            if (possibleTypes == null) throw new ArgumentNullException(nameof(possibleTypes));
            Syntax = syntax;
            PossibleTypes = possibleTypes.ToList();
            Escaped = true;
        }

        #endregion

        #region Properties

        public bool Escaped { get; set; }

        public List<Type> PossibleTypes { get; }

        public AttributeSyntax Syntax { get; }

        #endregion

        #region Override members

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((SuspiciousAttributeSyntax)obj);
        }

        public override int GetHashCode()
        {
            return Syntax.SyntaxTree.FilePath.GetHashCode().MergeHash(Syntax.FullSpan.GetHashCode());
        }

        #endregion

        #region Members

        protected bool Equals(SuspiciousAttributeSyntax other)
        {
            return GetHashCode() == other?.GetHashCode();
        }

        #endregion
    }
}