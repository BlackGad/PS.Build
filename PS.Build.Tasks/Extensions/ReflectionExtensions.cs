using System;
using System.Collections.Generic;
using System.Linq;

namespace PS.Build.Tasks.Extensions
{
    public static class ReflectionExtensions
    {
        #region Static members

        public static Dictionary<string, List<Type>> CreateAssociationMap(this IEnumerable<Type> types)
        {
            var result = new Dictionary<string, List<Type>>();
            foreach (var type in types)
            {
                var parts = type.FullName.Split('.');
                for (var i = 0; i < parts.Length; i++)
                {
                    var longForm = string.Join(".", parts.Skip(i));

                    result.Ensure(longForm, () => new List<Type>()).Add(type);

                    var postfix = "Attribute";
                    if (longForm.EndsWith(postfix))
                    {
                        var shortForm = longForm.Substring(0, longForm.Length - postfix.Length);
                        result.Ensure(shortForm, () => new List<Type>()).Add(type);
                    }
                }
            }
            return result;
        }

        #endregion
    }
}