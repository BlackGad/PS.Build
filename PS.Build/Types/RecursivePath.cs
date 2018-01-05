namespace PS.Build.Types
{
    public class RecursivePath
    {
        #region Properties

        public string Original { get; set; }
        public string PatternPostfix { get; set; }
        public string PatternPrefix { get; set; }
        public string PatternRecursive { get; set; }
        public string Postfix { get; set; }
        public string Prefix { get; set; }
        public string Recursive { get; set; }

        #endregion
    }
}