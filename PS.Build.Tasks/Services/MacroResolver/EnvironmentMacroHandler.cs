using System;
using System.ComponentModel.DataAnnotations;
using PS.Build.Types;

namespace PS.Build.Tasks.Services
{
    class EnvironmentMacroHandler : IMacroHandler
    {
        #region Properties

        string IMacroHandler.ID => nameof(Environment);

        int IMacroHandler.Order => 100;

        #endregion

        #region IMacroHandler Members

        bool IMacroHandler.CanHandle(string key, string value, string formatting)
        {
            return string.Equals(key, "env", StringComparison.InvariantCultureIgnoreCase);
        }

        HandledMacro IMacroHandler.Handle(string key, string value, string formatting)
        {
            if (string.IsNullOrWhiteSpace(value)) return new HandledMacro(new ValidationResult("Illegal environment variable"));
            var result = Environment.GetEnvironmentVariable(value);
            if (result == null) return new HandledMacro(new ValidationResult($"Environment variable '{value}' not found"));
            return new HandledMacro(result);
        }

        #endregion
    }
}