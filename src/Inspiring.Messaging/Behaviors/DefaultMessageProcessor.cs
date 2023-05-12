using Inspiring.Messaging.Behaviors.Phases;
using Inspiring.Messaging.Pipelines;
using System.Threading.Tasks;

namespace Inspiring.Messaging.Behaviors;

public class DefaultMessageProcessor<M, R, O, C> : PipelineStep<M, R, O, C, ProcessMessage<R>> {
    public override ProcessMessage<R> Invoke(Invocation<M, R, O, C> i, ProcessMessage<R> phase) {
        ProvideHandlers hs = i.Pipeline.Invoke(i, ProvideHandlers.Empty);
        InvokeHandlers<R> ih = i.Pipeline.Invoke(i, new InvokeHandlers<R>(hs.Handlers));
        AggregateResults<R> ar = i.Pipeline.Invoke(i, new AggregateResults<R>(ih.Results));

        return new ProcessMessage<R>(ar.Result);
    }

    public override async ValueTask<ProcessMessage<R>> InvokeAsync(Invocation<M, R, O, C> i, ProcessMessage<R> phase) {
        ProvideHandlers hs = await i.Pipeline.InvokeAsync(i, ProvideHandlers.Empty);
        InvokeHandlers<R> ih = await i.Pipeline.InvokeAsync(i, new InvokeHandlers<R>(hs.Handlers));
        AggregateResults<R> ar = await i.Pipeline.InvokeAsync(i, new AggregateResults<R>(ih.Results));

        return new ProcessMessage<R>(ar.Result);
    }
}
