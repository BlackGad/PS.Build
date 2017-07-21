using System.Collections.Generic;
using System.Linq;

namespace PS.Build.Tasks.Tests.Common.Extensions
{
    public static class MessagesExtensions
    {
        #region Static members

        public static IEnumerable<string> AssertContains(this IEnumerable<string> collection, int expectedCount, params string[] values)
        {
            var count = collection.Count(m =>
            {
                foreach (var value in values)
                {
                    if (!m.Contains(value)) return false;
                }
                return true;
            });
            if (count != expectedCount)
                return new[] { $"Expected messages '{string.Join("|", values)}' found in {count} records but expected {expectedCount}" };
            return Enumerable.Empty<string>();
        }

        #endregion
    }
}