namespace PS.Build.Types
{
    /// <summary>
    ///     Macro handler
    /// </summary>
    public interface IMacroHandler
    {
        #region Properties

        /// <summary>
        ///     Handler ID
        /// </summary>
        string ID { get; }

        /// <summary>
        ///     Handle order. Lower has higher priority.
        /// </summary>
        int Order { get; }

        #endregion

        #region Members

        /// <summary>
        ///     Called by macro service to determine ability to process input macro by this handler.
        /// </summary>
        /// <param name="key">Macro key.</param>
        /// <param name="value">Macro value.</param>
        /// <param name="formatting">Result formatting.</param>
        /// <returns>True if handler can process input macro; False otherwise.</returns>
        bool CanHandle(string key, string value, string formatting);

        /// <summary>
        ///     Process input macro. Called by macro after CanHandle returns true.
        /// </summary>
        /// <param name="key">Macro key.</param>
        /// <param name="value">Macro value.</param>
        /// <param name="formatting">Result formatting.</param>
        /// <returns>Process result.</returns>
        HandledMacro Handle(string key, string value, string formatting);

        #endregion
    }
}