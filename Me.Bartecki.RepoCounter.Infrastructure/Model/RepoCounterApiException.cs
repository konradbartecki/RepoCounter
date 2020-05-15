using System;
using System.Reflection;

namespace Me.Bartecki.RepoCounter.Infrastructure.Model
{
    public class RepoCounterApiException : Exception
    {

        public RepoCounterApiException(ErrorCodes error) : base()
        {
            this.ErrorCode = error;
        }

        public RepoCounterApiException(ErrorCodes error, string message) : base(message)
        {
            this.ErrorCode = error;
            //This exception is also used for user error reporting,
            //so we want to capture a stacktrace on construction instead of on throw
            //because this exception may never be thrown, but just returned to the client.
            var stackTraceField = typeof(RepoCounterApiException).BaseType
                .GetField("_stackTraceString", BindingFlags.Instance | BindingFlags.NonPublic);
            stackTraceField.SetValue(this, Environment.StackTrace);
        }

        public RepoCounterApiException(ErrorCodes error, string message, Exception innerException) : base(message, innerException)
        {
            this.ErrorCode = error;
            //This exception is also used for user error reporting,
            //so we want to capture a stacktrace on construction instead of on throw
            //because this exception may never be thrown, but just returned to the client.
            var stackTraceField = typeof(RepoCounterApiException).BaseType
                .GetField("_stackTraceString", BindingFlags.Instance | BindingFlags.NonPublic);
            stackTraceField.SetValue(this, Environment.StackTrace);
        }

        public ErrorCodes ErrorCode { get; }
    }
}
