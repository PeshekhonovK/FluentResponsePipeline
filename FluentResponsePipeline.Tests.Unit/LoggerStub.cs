using System.Collections.Generic;
using FluentResponsePipeline.Contracts.Public;

namespace FluentResponsePipeline.Tests.Unit
{
    public class LoggerStub : IObjectLogger
    {
        private readonly List<object> trace = new List<object>();
        private readonly List<object> debug  = new List<object>();
        private readonly List<object> information  = new List<object>();
        private readonly List<object> warning  = new List<object>();
        private readonly List<object> error  = new List<object>();
        private readonly List<object> critical  = new List<object>();
        private readonly List<string> messages  = new List<string>();

        public IReadOnlyCollection<object> Trace => this.trace;

        public IReadOnlyCollection<object> Debug => this.debug;

        public IReadOnlyCollection<object> Information => this.information;

        public IReadOnlyCollection<object> Warning => this.warning;

        public IReadOnlyCollection<object> Error => this.error;

        public IReadOnlyCollection<object> Critical => this.critical;

        public IReadOnlyCollection<string> Messages => this.messages;

        public void LogTrace<TObject>(TObject result)
        {
            this.trace.Add(result);
        }

        public void LogDebug<TObject>(TObject result)
        {
            this.debug.Add(result);
        }

        public void LogInformation<TObject>(TObject result)
        {
            this.information.Add(result);
        }

        public void LogWarning<TObject>(TObject result)
        {
            this.warning.Add(result);
        }

        public void LogError<TObject>(TObject result)
        {
            this.error.Add(result);
        }

        public void LogCritical<TObject>(TObject result)
        {
            this.critical.Add(result);
        }

        public void LogTrace<TObject>(string message, TObject result)
        {
            this.trace.Add(result);
            this.messages.Add(message);
        }

        public void LogDebug<TObject>(string message, TObject result)
        {
            this.debug.Add(result);
            this.messages.Add(message);
        }

        public void LogInformation<TObject>(string message, TObject result)
        {
            this.information.Add(result);
            this.messages.Add(message);
        }

        public void LogWarning<TObject>(string message, TObject result)
        {
            this.warning.Add(result);
            this.messages.Add(message);
        }

        public void LogError<TObject>(string message, TObject result)
        {
            this.error.Add(result);
            this.messages.Add(message);
        }

        public void LogCritical<TObject>(string message, TObject result)
        {
            this.critical.Add(result);
            this.messages.Add(message);
        }
    }
}