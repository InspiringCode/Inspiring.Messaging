using Inspiring.Messaging.Behaviors.Phases;
using Inspiring.Messaging.Pipelines;
using System.Threading.Tasks;

namespace Inspiring.Messaging.Behaviors;
public class FirstOrDefaultResultAggregator<M, R, O, C> : PipelineStep<M, R, O, C, AggregateResults<R>> {
    public override AggregateResults<R> Invoke(Invocation<M, R, O, C> i, AggregateResults<R> phase) {
        R[] rs = phase.Results;

        for (int j = 0; j < rs.Length; j++)
            if (typeof(R).IsValueType || rs[j] != null)
                return phase with { Result = rs[j] };

        return phase with { Result = default };
    }

    public override ValueTask<AggregateResults<R>> InvokeAsync(Invocation<M, R, O, C> i, AggregateResults<R> phase) {
        return new ValueTask<AggregateResults<R>>(Invoke(i, phase));
    }
}
