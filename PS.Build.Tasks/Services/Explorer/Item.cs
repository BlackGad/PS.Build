using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Build.Framework;
using PS.Build.Tasks.Extensions;
using PS.Build.Types;

namespace PS.Build.Tasks.Services
{
    public class Item : MarshalByRefObject,
                        IItem
    {
        #region Property definitions

        public static readonly string[] ReservedPropertyNames;

        #endregion

        #region Constructors

        static Item()
        {
            ReservedPropertyNames = typeof(Item).GetProperties()
                                                .Select(p => p.Name)
                                                .Except(new[] { nameof(Metadata) })
                                                .ToArray();
        }

        public Item(ITaskItem taskItem)
        {
            if (taskItem == null) throw new ArgumentNullException(nameof(taskItem));
            Metadata = new ReadOnlyDictionary<string, string>(taskItem.MetadataNames
                                                                      .OfType<string>()
                                                                      .ToDictionary(n => n, taskItem.GetMetadata));
        }

        #endregion

        #region Properties

        public DateTime AccessedTime
        {
            get
            {
                DateTime value;
                DateTime.TryParse(Metadata.Get(nameof(AccessedTime)), out value);
                return value;
            }
        }

        public DateTime CreatedTime
        {
            get
            {
                DateTime value;
                DateTime.TryParse(Metadata.Get(nameof(CreatedTime)), out value);
                return value;
            }
        }

        public string Directory => Metadata.Get(nameof(Directory));
        public string Extension => Metadata.Get(nameof(Extension));
        public string Filename => Metadata.Get(nameof(Filename));

        public string FullPath => Metadata.Get(nameof(FullPath));
        public string Identity => Metadata.Get(nameof(Identity));

        public IReadOnlyDictionary<string, string> Metadata { get; }

        public DateTime ModifiedTime
        {
            get
            {
                DateTime value;
                DateTime.TryParse(Metadata.Get(nameof(ModifiedTime)), out value);
                return value;
            }
        }

        public string RecursiveDir => Metadata.Get(nameof(RecursiveDir));
        public string RelativeDir => Metadata.Get(nameof(RelativeDir));
        public string RootDir => Metadata.Get(nameof(RootDir));

        #endregion
    }
}