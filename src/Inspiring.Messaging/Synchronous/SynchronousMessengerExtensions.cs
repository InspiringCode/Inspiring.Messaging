using Inspiring.Messaging.Behaviors.Phases;
using Inspiring.Messaging.Pipelines;

namespace Inspiring.Messaging.Synchronous;

public static class SynchronousMessengerExtensions {
    public static R SendSync<M, R>(this IMessenger messenger, IMessage<M, R> message) where M : IMessage<M, R> {
        ProcessMessage<R> phase = messenger.Dispatcher.Dispatch<M, R, SendOperation, ProcessMessage<R>>(
            (M)message,
            SendOperation.Instance,
            phase: new ProcessMessage<R>(default!));

        return phase.Result!;
    }
}
