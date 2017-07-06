namespace PS.Build.Services
{
    public interface ILogger
    {
        #region Members

        void Critical(string message);
        void Debug(string message);
        void Error(string message);
        void Info(string message);
        void Warn(string message);

        #endregion
    }
}