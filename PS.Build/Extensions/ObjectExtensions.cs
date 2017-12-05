namespace PS.Build.Extensions
{
    public static class ObjectExtensions
    {
        #region Static members

        public static int MergeHash(this int hash, int addHash)
        {
            return (hash*397) ^ addHash;
        }

        #endregion
    }
}