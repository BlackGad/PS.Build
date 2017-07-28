using System;
using System.ComponentModel.DataAnnotations;
using PS.Build.Types;

namespace PS.Build.Tasks.Services
{
    class SpecialDirectoryMacroHandler : IMacroHandler
    {
        #region Properties

        public string ID => "SpecialFolder";

        int IMacroHandler.Order => 100;

        #endregion

        #region IMacroHandler Members

        bool IMacroHandler.CanHandle(string key, string value, string formatting)
        {
            return string.Equals(key, "sdir", StringComparison.InvariantCultureIgnoreCase);
        }

        HandledMacro IMacroHandler.Handle(string key, string value, string formatting)
        {
            if (string.IsNullOrWhiteSpace(value)) return new HandledMacro(new ValidationResult($"Invalid {ID} option"));

            Environment.SpecialFolder specialFolder;
            if (!Enum.TryParse(value, true, out specialFolder))
            {
                return new HandledMacro(new ValidationResult($"Not supported '{value}' folder"));
            }
            return new HandledMacro(Environment.GetFolderPath(specialFolder));
        }

        #endregion
    }
}