using Inspiring.Messaging.Pipelines;
using Inspiring.Messaging.Core;
using System;
using System.Threading.Tasks;
using Inspiring.Messaging.Behaviors.Phases;

namespace Inspiring.Messaging
{
    public class Messenger : IMessenger {
        public IMessageDispatcher Dispatcher { get; }

        public Messenger(IServiceProvider services)
            : this(new MessageDispatcher(services)) { }

        public Messenger(IServiceProvider services, PipelineFactory factory, IPipelineCache cache)
            : this(new MessageDispatcher(services, factory, cache)) { }

        public Messenger(IMessageDispatcher dispatcher) {
            Dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        }

        public async ValueTask<R> SendAsync<M, R>(IMessage<M, R> message) where M : IMessage<M, R> {
            ProcessMessage<R> phase = await Dispatcher.DispatchAsync<M, R, SendOperation, ProcessMessage<R>>(
                (M)message,
                SendOperation.Instance,
                phase: new ProcessMessage<R>(default!));

            return phase.Result!;
        }
    }
}
