using System;
using System.Runtime.Serialization;

namespace Inspiring.Messaging;

[Serializable]
public class MessengerException : Exception {
    public MessengerException() { }

    public MessengerException(string message) : base(message) { }

    public MessengerException(string message, Exception inner) : base(message, inner) { }

    protected MessengerException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
