using System;
using System.Collections.Generic;

namespace PS.Build.Types
{
    /// <summary>
    ///     Wrapped MSBuild item.
    /// </summary>
    public interface IItem
    {
        #region Properties

        /// <summary>
        ///     Contains the timestamp from the last time the item was accessed.
        /// </summary>
        DateTime AccessedTime { get; }

        /// <summary>
        ///     Contains the timestamp from when the item was created.
        /// </summary>
        DateTime CreatedTime { get; }

        /// <summary>
        ///     Contains the directory of the item, without the root directory.
        /// </summary>
        /// <example>
        ///     MyProject\Source\
        /// </example>
        string Directory { get; }

        /// <summary>
        ///     Contains the file name extension of the item.
        /// </summary>
        /// <example>
        ///     .cs
        /// </example>
        string Extension { get; }

        /// <summary>
        ///     Contains the file name of the item, without the extension.
        /// </summary>
        /// <example>
        ///     Program
        /// </example>
        string Filename { get; }

        /// <summary>
        ///     Contains the full path of the item.
        /// </summary>
        /// <example>
        ///     C:\MyProject\Source\Program.cs
        /// </example>
        string FullPath { get; }

        /// <summary>
        ///     The item specified in the Include attribute.
        /// </summary>
        /// <example>
        ///     Source\Program.cs
        /// </example>
        string Identity { get; }

        /// <summary>
        ///     Metadata assigned to every item upon creation.
        /// </summary>
        IReadOnlyDictionary<string, string> Metadata { get; }

        /// <summary>
        ///     Contains the timestamp from the last time the item was modified.
        /// </summary>
        DateTime ModifiedTime { get; }

        /// <summary>
        ///     If the Include attribute contains the wildcard **, this metadata specifies the part of the path that replaces the
        ///     wildcard. For more information on wildcards, <see cref="https://msdn.microsoft.com/en-us/library/ms171454.aspx" />
        ///     How to: Select the Files to Build.
        /// </summary>
        /// <example>
        ///     If the folder C:\MySolution\MyProject\Source\ contains the file Program.cs, and if the project file contains this
        ///     item:
        ///     &lt;ItemGroup&gt;
        ///     &lt;MyItem Include = "C:\**\Program.cs" /&gt;
        ///     &lt;/ ItemGroup &gt;
        ///     then the value of %(MyItem.RecursiveDir) would be MySolution\MyProject\Source\.
        /// </example>
        string RecursiveDir { get; }

        /// <summary>
        ///     Contains the path specified in the Include attribute, up to the final backslash (\).
        /// </summary>
        /// <example>
        ///     Source\
        /// </example>
        string RelativeDir { get; }

        /// <summary>
        ///     Contains the root directory of the item.
        /// </summary>
        /// <example>
        ///     C:\
        /// </example>
        string RootDir { get; }

        #endregion
    }
}