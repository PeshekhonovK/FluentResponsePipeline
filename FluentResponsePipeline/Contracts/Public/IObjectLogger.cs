namespace FluentResponsePipeline.Contracts.Public
{
    public interface IObjectLogger
    {
        void LogTrace<TObject>(TObject result);
        void LogDebug<TObject>(TObject result);
        void LogInformation<TObject>(TObject result);
        void LogWarning<TObject>(TObject result);
        void LogError<TObject>(TObject result);
        void LogCritical<TObject>(TObject result);
        void LogTrace<TObject>(string message, TObject result);
        void LogDebug<TObject>(string message, TObject result);
        void LogInformation<TObject>(string message, TObject result);
        void LogWarning<TObject>(string message, TObject result);
        void LogError<TObject>(string message, TObject result);
        void LogCritical<TObject>(string message, TObject result);
    }

    public interface IObjectLogger<TClass> : IObjectLogger
    {
    }
}