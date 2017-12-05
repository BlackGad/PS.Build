using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PS.Build.Tasks
{
    class SuspiciousAttributeSyntaxes : IEnumerable<SuspiciousAttributeSyntax>
    {
        readonly List<SuspiciousAttributeSyntax> _syntaxes;

        #region Constructors

        public SuspiciousAttributeSyntaxes()
        {
            _syntaxes = new List<SuspiciousAttributeSyntax>();
        }

        #endregion

        #region Properties

        public int Count
        {
            get { return _syntaxes.Count; }
        }

        #endregion

        #region IEnumerable<SuspiciousAttributeSyntax> Members

        public IEnumerator<SuspiciousAttributeSyntax> GetEnumerator()
        {
            return _syntaxes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Members

        public void Manage(AttributeSyntax syntax, IEnumerable<Type> types)
        {
            var newRecord = new SuspiciousAttributeSyntax(syntax, types);
            var existing = _syntaxes.FirstOrDefault(s => s == newRecord);
            if (existing != null) existing.Escaped = false;
            else _syntaxes.Add(newRecord);
        }

        #endregion
    }
}