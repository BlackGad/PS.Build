using System;
using System.ComponentModel.DataAnnotations;
using PS.Build.Services;
using PS.Build.Types;

namespace PS.Build.Tasks.Services
{
    class ExplorerMacroHandler : IMacroHandler
    {
        private readonly IExplorer _explorer;

        #region Constructors

        public ExplorerMacroHandler(IExplorer explorer)
        {
            if (explorer == null) throw new ArgumentNullException(nameof(explorer));
            _explorer = explorer;
        }

        #endregion

        #region Properties

        string IMacroHandler.ID
        {
            get { return nameof(Explorer); }
        }

        int IMacroHandler.Order
        {
            get { return 100; }
        }

        #endregion

        #region IMacroHandler Members

        bool IMacroHandler.CanHandle(string key, string value, string formatting)
        {
            return string.Equals(key, "prop", StringComparison.InvariantCultureIgnoreCase) ||
                   string.Equals(key, "dir", StringComparison.InvariantCultureIgnoreCase);
        }

        HandledMacro IMacroHandler.Handle(string key, string value, string formatting)
        {
            switch (key.ToLowerInvariant())
            {
                case "prop":
                {
                    BuildProperty property;
                    return Enum.TryParse(value, true, out property)
                        ? new HandledMacro(_explorer.Properties[property])
                        : new HandledMacro(new ValidationResult($"Unknown '{value}' property."));
                }
                case "dir":
                {
                    BuildDirectory directory;
                    return Enum.TryParse(value, true, out directory)
                        ? new HandledMacro(_explorer.Directories[directory])
                        : new HandledMacro(new ValidationResult($"Unknown '{value}' directory."));
                }
                default:
                    throw new NotSupportedException();
            }
        }

        #endregion
    }
}