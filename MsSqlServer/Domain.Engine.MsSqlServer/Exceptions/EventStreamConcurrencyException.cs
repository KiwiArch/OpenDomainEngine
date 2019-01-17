namespace Ode.Domain.Engine.MsSqlServer.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class EventStreamConcurrencyException : Exception
    {
        public EventStreamConcurrencyException()
            : base()
        {
            return;
        }

        public EventStreamConcurrencyException(string streamId, int attemptedNextVersion, int expectedNextVersion)
            : base(message: $"Attempt to save event with version {attemptedNextVersion} into stream {streamId} with expected next version of {expectedNextVersion}.")
        {
            return;
        }

        public EventStreamConcurrencyException(string message)
            : base(message)
        {
            return;
        }

        public EventStreamConcurrencyException(string message, Exception innerException)
            : base(message, innerException)
        {
            return;
        }

        private EventStreamConcurrencyException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            return;
        }

    }
}
