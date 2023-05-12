using System.Threading.Tasks;

namespace Inspiring.Messaging.Pipelines.Internal;

internal class NoOpStep<M, R, O, C, P> : PipelineStep<M, R, O, C, P> {
    public static readonly NoOpStep<M, R, O, C, P> Default = new();

    public override P Invoke(Invocation<M, R, O, C> invocation, P phase)
        => phase;

    public override ValueTask<P> InvokeAsync(Invocation<M, R, O, C> invocation, P phase)
        => new ValueTask<P>(phase);
}