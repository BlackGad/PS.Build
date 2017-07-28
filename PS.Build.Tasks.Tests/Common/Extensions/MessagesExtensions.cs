using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Build.Framework;
using PS.Build.Tasks.Extensions;

namespace PS.Build.Tasks.Tests.Common.Extensions
{
    public static class MessagesExtensions
    {
        #region Static members

        public static IEnumerable<string> AssertContains(this IEnumerable<LazyFormattedBuildEventArgs> collection,
                                                         int expectedCount,
                                                         params string[] values)
        {
            var count = collection.Count(m =>
            {
                foreach (var value in values)
                {
                    if (m.Message.IndexOf(value, StringComparison.InvariantCultureIgnoreCase) == -1) return false;
                }
                return true;
            });
            if (count != expectedCount)
                return new[] { $"Expected messages '{string.Join("|", values)}' found in {count} records but expected {expectedCount}" };
            return Enumerable.Empty<string>();
        }

        public static IEnumerable<string> AssertEmpty(this IEnumerable<LazyFormattedBuildEventArgs> collection)
        {
            var postErrors = collection.Enumerate().Select(m => m.Message).ToList();
            if (postErrors.Any()) return postErrors;
            return Enumerable.Empty<string>();
        }

        #endregion
    }
}