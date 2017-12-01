using System.Collections.Generic;
using System.Linq;

namespace PS.Build.Types
{
    public class SearchResult<T>
    {
        #region Constructors

        public SearchResult() : this(null, null)
        {
        }

        public SearchResult(string pattern, IEnumerable<T> items)
        {
            Items = items ?? Enumerable.Empty<T>();
            Pattern = pattern ?? string.Empty;
        }

        #endregion

        #region Properties

        public IEnumerable<T> Items { get; }

        public string Pattern { get; }

        #endregion
    }
}