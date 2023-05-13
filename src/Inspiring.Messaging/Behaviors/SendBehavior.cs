using Inspiring.Messaging.Behaviors.Phases;
using Inspiring.Messaging.Pipelines;
using System;
using System.Threading.Tasks;

namespace Inspiring.Messaging.Behaviors;

public sealed class SendBehavior : IMessageBehavior {
    public void Configure<M, R, O, C>(PipelineBuilder<M, R, O, C> pipeline) {
        pipeline.AddStep<InvokeHandlers<R>>(next => new RequireAtLeastOneHandlerStep<M, R, O, C>(next));
    }

    private class RequireAtLeastOneHandlerStep<M, R, O, C> : PipelineStep<M, R, O, C, InvokeHandlers<R>> {
        private readonly PipelineStep<M, R, O, C, InvokeHandlers<R>> _next;

        public RequireAtLeastOneHandlerStep(PipelineStep<M, R, O, C, InvokeHandlers<R>> next)
            => _next = next;

        public override InvokeHandlers<R> Invoke(Invocation<M, R, O, C> i, InvokeHandlers<R> phase) {
            phase = _next.Invoke(i, phase);
            if (phase.Handlers.Length == 0) ThrowNoHandlerRegisteredException(true);
            return phase;
        }

        public async override ValueTask<InvokeHandlers<R>> InvokeAsync(Invocation<M, R, O, C> i, InvokeHandlers<R> phase) {
            phase = await _next.InvokeAsync(i, phase);
            if (phase.Handlers.Length == 0) ThrowNoHandlerRegisteredException(true);
            return phase;
        }

        private void ThrowNoHandlerRegisteredException(bool async) {
            string additionalAsyncText = async ?
                $"or 'IHandlesAsync<{typeof(M).Name}, {typeof(R).Name}>' " :
                "";

            throw new MessengerException($"No handler is registered for message '{typeof(M).Name}' but " +
                $"a 'Send' operation requires at least one handler. Make sure that you have at least " +
                $"one type that implements 'IHandles<{typeof(M).Name}, {typeof(R).Name}>' {additionalAsyncText}" +
                $"registered as 'IHandles<{typeof(M).Name}>' (this is important, since the messenger resolves " +
                $"the handlers by 'IHandles<M>' and not by 'IHandles<M, R>' or 'IHandlesAsync<M, R>). " +
                $"Alternatively you can use the 'Publish' operation which does not require a message " +
                $"handler to be registered.");
        }
    }
}
