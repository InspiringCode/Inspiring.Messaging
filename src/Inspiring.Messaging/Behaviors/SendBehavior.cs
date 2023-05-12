using Inspiring.Messaging.Behaviors.Phases;
using Inspiring.Messaging.Pipelines;
using System;
using System.Threading.Tasks;

namespace Inspiring.Messaging.Behaviors;

public sealed class SendBehavior : IMessageBehavior {
    public void Configure<M, R, O, C>(PipelineBuilder<M, R, O, C> pipeline) {
        pipeline.AddStep<InvokeHandlers<R>>(next => new SendStep<M, R, O, C>(next));
    }

    private class SendStep<M, R, O, C> : PipelineStep<M, R, O, C, InvokeHandlers<R>> {
        private readonly PipelineStep<M, R, O, C, InvokeHandlers<R>> _next;

        public SendStep(PipelineStep<M, R, O, C, InvokeHandlers<R>> next)
            => _next = next;

        public override InvokeHandlers<R> Invoke(Invocation<M, R, O, C> i, InvokeHandlers<R> phase) {
            throw new NotImplementedException();
        }

        public override ValueTask<InvokeHandlers<R>> InvokeAsync(Invocation<M, R, O, C> i, InvokeHandlers<R> phase) {
            throw new NotImplementedException();
        }
    }
}
