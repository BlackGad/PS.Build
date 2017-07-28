using System;
using System.ComponentModel.DataAnnotations;
using PS.Build.Types;

namespace PS.Build.Tasks.Services
{
    class TimeMacroHandler : IMacroHandler
    {
        #region Properties

        public string ID => "time";

        int IMacroHandler.Order => 100;

        #endregion

        #region IMacroHandler Members

        bool IMacroHandler.CanHandle(string key, string value, string formatting)
        {
            return string.Equals(key, "time", StringComparison.InvariantCultureIgnoreCase);
        }

        HandledMacro IMacroHandler.Handle(string key, string value, string formatting)
        {
            if (string.IsNullOrWhiteSpace(value)) return new HandledMacro(new ValidationResult($"Invalid {ID} option"));
            switch (value.ToLowerInvariant())
            {
                case "now":
                    return new HandledMacro(DateTimeOffset.Now.ToString(formatting));
            }

            return new HandledMacro(new ValidationResult($"Not supported {ID} option"));
        }

        #endregion
    }
}