using System.Collections.Generic;
using PS.Build.Types;

namespace PS.Build.Services
{
    /// <summary>
    ///     Explorer service interface
    /// </summary>
    public interface IExplorer
    {
        #region Properties

        /// <summary>
        ///     Gets the path to the special folder that is identified by the specified enumeration.
        /// </summary>
        IReadOnlyDictionary<BuildDirectory, string> Directories { get; }

        /// <summary>
        ///     Gets MSBuild item list grouped by the specified enumeration.
        /// </summary>
        IReadOnlyDictionary<BuildItem, IReadOnlyList<IItem>> Items { get; }

        /// <summary>
        ///     Gets MSBuild specific properties that is identified by the specified enumeration.
        /// </summary>
        IReadOnlyDictionary<BuildProperty, string> Properties { get; }

        /// <summary>
        ///     Gets MSBuild Refererence item list.
        /// </summary>
        IReadOnlyList<IItem> References { get; }

        #endregion
    }
}