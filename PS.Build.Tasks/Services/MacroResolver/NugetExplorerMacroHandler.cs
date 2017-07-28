using System;
using System.ComponentModel.DataAnnotations;
using PS.Build.Services;
using PS.Build.Types;

namespace PS.Build.Tasks.Services
{
    class NugetExplorerMacroHandler : IMacroHandler
    {
        private readonly INugetExplorer _nugetExplorer;

        #region Constructors

        public NugetExplorerMacroHandler(INugetExplorer nugetExplorer)
        {
            if (nugetExplorer == null) throw new ArgumentNullException(nameof(nugetExplorer));
            _nugetExplorer = nugetExplorer;
        }

        #endregion

        #region Properties

        string IMacroHandler.ID => nameof(NugetExplorer);

        int IMacroHandler.Order => 100;

        #endregion

        #region IMacroHandler Members

        bool IMacroHandler.CanHandle(string key, string value, string formatting)
        {
            return string.Equals(key, "nuget", StringComparison.InvariantCultureIgnoreCase);
        }

        HandledMacro IMacroHandler.Handle(string key, string value, string formatting)
        {
            try
            {
                value = value ?? string.Empty;
                var commandPosition = value.IndexOf("@", StringComparison.InvariantCultureIgnoreCase);
                var command = "dir";
                var id = value;

                if (commandPosition != -1)
                {
                    command = value.Substring(commandPosition + 1).ToLowerInvariant();
                    id = value.Substring(0, commandPosition);
                }

                var package = _nugetExplorer.FindPackage(id);
                if (package == null) return new HandledMacro(new ValidationResult($"Package '{id}' not found."));
                switch (command)
                {
                    case "dir":
                        return new HandledMacro(package.Folder);
                    case "ver":
                    {
                        var result = package.Version.ToString();
                        int fieldCount;
                        if (!string.IsNullOrWhiteSpace(formatting) && int.TryParse(formatting, out fieldCount))
                        {
                            result = package.Version.ToString(fieldCount);
                        }
                        return new HandledMacro(result);
                    }
                    case "id":
                        return new HandledMacro(package.ID);
                    default:
                        return new HandledMacro(new ValidationResult($"Command '{command}' not supported"));
                }
            }
            catch (Exception e)
            {
                return new HandledMacro(new ValidationResult(e.GetBaseException().Message));
            }
        }

        #endregion
    }
}