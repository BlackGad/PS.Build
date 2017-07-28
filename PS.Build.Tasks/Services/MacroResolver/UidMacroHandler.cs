using System;
using System.ComponentModel.DataAnnotations;
using PS.Build.Types;

namespace PS.Build.Tasks.Services
{
    class UidMacroHandler : IMacroHandler
    {
        #region Properties

        public string ID => "uid";

        int IMacroHandler.Order => 100;

        #endregion

        #region IMacroHandler Members

        bool IMacroHandler.CanHandle(string key, string value, string formatting)
        {
            return string.Equals(key, "uid", StringComparison.InvariantCultureIgnoreCase);
        }

        HandledMacro IMacroHandler.Handle(string key, string value, string formatting)
        {
            if (string.IsNullOrWhiteSpace(value)) return new HandledMacro(new ValidationResult($"Invalid {ID} option"));
            switch (value.ToLowerInvariant())
            {
                case "empty":
                    return new HandledMacro(Guid.Empty.ToString(formatting));
                case "new":
                    return new HandledMacro(Guid.NewGuid().ToString(formatting));
            }

            return new HandledMacro(new ValidationResult($"Not supported {ID} option"));
        }

        #endregion
    }
}