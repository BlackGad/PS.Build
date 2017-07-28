using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using PS.Build.Services;
using PS.Build.Types;

namespace PS.Build.Tasks.Services
{
    class MacroResolver : IMacroResolver
    {
        #region Constants

        private const string MacroFormatGroup = nameof(MacroFormatGroup);

        private const string MacroGroup = nameof(MacroGroup);
        private const string MacroKeyGroup = nameof(MacroKeyGroup);
        private const string MacroValueGroup = nameof(MacroValueGroup);

        #endregion

        private readonly List<IMacroHandler> _handlers;

        #region Constructors

        public MacroResolver()
        {
            _handlers = new List<IMacroHandler>();
        }

        #endregion

        #region IMacroResolver Members

        public void Register(IMacroHandler handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            var id = handler.ID;
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Handler ID is not set");
            if (_handlers.Any(h => h.ID == id)) return;
            _handlers.Add(handler);
        }

        public string Resolve(string source, out ValidationResult[] errors)
        {
            var matchPattern = $"(?<{MacroGroup}>{{[^}}]+}})";

            var resultErrors = new List<ValidationResult>();
            var result = Regex.Replace(source, matchPattern, GetMatchEvaluator(resultErrors));
            errors = resultErrors.Any() ? resultErrors.ToArray() : null;
            return result;
        }

        #endregion

        #region Members

        private MatchEvaluator GetMatchEvaluator(List<ValidationResult> errors)
        {
            return match =>
            {
                var value = match.Value;
                var localPattern = $"{{(?<{MacroKeyGroup}>[^\\.\\:\\}}]+)(\\.(?<{MacroValueGroup}>[^\\:\\}}]+))?(:(?<{MacroFormatGroup}>[^\\}}]+))?}}";
                var localMatch = Regex.Match(value,
                                             localPattern,
                                             RegexOptions.IgnorePatternWhitespace | RegexOptions.CultureInvariant | RegexOptions.Singleline);
                if (!localMatch.Success || !localMatch.Groups[MacroKeyGroup].Success)
                {
                    errors.Add(new ValidationResult($"Macro '{value}' has invalid format. Expected: {{KEY[.VALUE][:FORMAT]}}"));
                    return value;
                }

                var macroKey = localMatch.Groups[MacroKeyGroup].Value;
                var macroValue = localMatch.Groups[MacroValueGroup].Success ? localMatch.Groups[MacroValueGroup].Value : null;
                var macroFromatting = localMatch.Groups[MacroFormatGroup].Success ? localMatch.Groups[MacroFormatGroup].Value : null;

                var orderedHandlers = _handlers.OrderBy(h =>
                {
                    try
                    {
                        return h.Order;
                    }
                    catch (Exception)
                    {
                        return int.MaxValue;
                    }
                });

                var handler = orderedHandlers.FirstOrDefault(h =>
                {
                    try
                    {
                        return h.CanHandle(macroKey, macroValue, macroFromatting);
                    }
                    catch
                    {
                        return false;
                    }
                });

                if (handler == null)
                {
                    errors.Add(new ValidationResult($"Macro '{value}' could not be handled by any macro handler."));
                    return value;
                }

                try
                {
                    var handledResult = handler.Handle(macroKey, macroValue, macroFromatting);
                    if (handledResult.ValidationErrors != null)
                    {
                        errors.AddRange(handledResult.ValidationErrors);
                        return value;
                    }

                    return handledResult.Result;
                }
                catch (Exception e)
                {
                    errors.Add(new ValidationResult($"Macro handler '{handler.ID}' filed. Details: {e.GetBaseException().Message}"));
                    return value;
                }
            };
        }

        #endregion
    }
}