using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using PS.Build.Services;
using PS.Build.Types;

namespace PS.Build.Tasks.Services
{
    class Explorer : MarshalByRefObject,
                     IExplorer
    {
        #region Constructors

        public Explorer(IEnumerable<Item> references,
                        IDictionary<BuildItem, IEnumerable<Item>> items,
                        IDictionary<BuildDirectory, string> folders,
                        IDictionary<BuildProperty, string> properties)
        {
            Directories = folders.ToDictionary(p => p.Key, p => p.Value);
            Properties = properties.ToDictionary(p => p.Key, p => p.Value);
            References = new ReadOnlyCollection<Item>(references.ToList());
            Items = items.ToDictionary(p => p.Key,
                                       p => (IReadOnlyList<IItem>)new ReadOnlyCollection<IItem>(p.Value.OfType<IItem>().ToList()));
        }

        #endregion

        #region Properties

        public IReadOnlyDictionary<BuildDirectory, string> Directories { get; }
        public IReadOnlyDictionary<BuildItem, IReadOnlyList<IItem>> Items { get; }

        public IReadOnlyDictionary<BuildProperty, string> Properties { get; }
        public IReadOnlyList<IItem> References { get; }

        #endregion
    }
}