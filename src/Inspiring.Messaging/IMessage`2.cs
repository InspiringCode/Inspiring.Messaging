using Inspiring.Messaging.Core;

namespace Inspiring.Messaging;

public interface IMessage<out M, in R> : IMessage where M : IMessage<M, R> { }
