using Inspiring.Messaging.Behaviors.Phases;
using Inspiring.Messaging.Core;
using Inspiring.Messaging.Pipelines;
using System;
using System.Threading.Tasks;

namespace Inspiring.Messaging.Behaviors;

public class DefaultHandlersInvoker<M, R, O, C> : PipelineStep<M, R, O, C, InvokeHandlers<R>> {
    public override InvokeHandlers<R> Invoke(Invocation<M, R, O, C> i, InvokeHandlers<R> phase) {
        IHandler[] hs = phase.Handlers;
        R?[] results = new R?[hs.Length];
        
        for (int j = 0; j < hs.Length; j++) {
            results[j] = i
                .Pipeline.Invoke(i, new InvokeHandler<R>(hs[j]))
                .Result; 
        }

        return phase with { Results = results };
    }

    public override async ValueTask<InvokeHandlers<R>> InvokeAsync(Invocation<M, R, O, C> i, InvokeHandlers<R> phase) {
        IHandler[] hs = phase.Handlers;
        R?[] results = new R?[hs.Length];

        for (int j = 0; j < hs.Length; j++) {
            InvokeHandler<R> ih = await i.Pipeline.InvokeAsync(i, new InvokeHandler<R>(hs[j]));
            results[j] = ih.Result;
        }

        return phase with { Results = results };
    }
}
