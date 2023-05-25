using Inspiring.Messaging.Behaviors.Phases;
using Inspiring.Messaging.Pipelines;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Inspiring.Messaging.Behaviors;

public class DefaultHandlerInvoker<M, R, O, C> : PipelineStep<M, R, O, C, InvokeHandler<R>> where M : IMessage<M, R> {
    public override InvokeHandler<R> Invoke(Invocation<M, R, O, C> i, InvokeHandler<R> phase) {
        return phase.Handler is IHandles<M, R> h ?
            phase with { Result = h.Handle(i.Message) } :
            phase;
    }

    public override async ValueTask<InvokeHandler<R>> InvokeAsync(Invocation<M, R, O, C> i, InvokeHandler<R> phase) {
        if (phase.Handler is IHandlesAsync<M, R> h)
            return phase with { Result = await h.Handle(i.Message) };

        if (phase.Handler is IHandles<M, R> hs)
            return phase with { Result = hs.Handle(i.Message) };

        return phase;
    }
}
