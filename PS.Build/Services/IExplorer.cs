using System.Collections.Generic;
using PS.Build.Types;

namespace PS.Build.Services
{
    public interface IExplorer
    {
        #region Properties

        IReadOnlyDictionary<BuildDirectory, string> Directories { get; }
        IReadOnlyDictionary<BuildItem, IReadOnlyList<IItem>> Items { get; }
        IReadOnlyDictionary<BuildProperty, string> Properties { get; }
        IReadOnlyList<IItem> References { get; }

        #endregion
    }
}