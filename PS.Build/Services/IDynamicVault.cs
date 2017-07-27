namespace PS.Build.Services
{
    /// <summary>
    ///     Dynamic data vault service interface
    /// </summary>
    public interface IDynamicVault
    {
        #region Members

        /// <summary>
        ///     Query stored data from vault.
        /// </summary>
        /// <param name="key">Data key.</param>
        /// <param name="value">Data value.</param>
        /// <returns>A return value indicates whether the data with specified key was stored.</returns>
        bool Query(object key, out object value);

        /// <summary>
        ///     Store data in the vault.
        /// </summary>
        /// <param name="key">Data key.</param>
        /// <param name="value">Data value.</param>
        /// <returns>Data value for fluent interaction.</returns>
        object Store(object key, object value);

        #endregion
    }
}