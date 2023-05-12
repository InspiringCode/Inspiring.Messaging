using Inspiring.Messaging.Core;

namespace Inspiring.Messaging;

public interface IHandles<in M, out R> : IHandles<M> where M : IMessage<M, R> {
    R Handle(M message);
}
