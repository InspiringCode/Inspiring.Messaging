using Inspiring.Messaging.Behaviors.Phases;
using Inspiring.Messaging.Core;
using Inspiring.Messaging.Pipelines;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Inspiring.Messaging.Behaviors;
public class ContainerHandlerProvider<M, R, O, C> : PipelineStep<M, R, O, C, ProvideHandlers> where C : IServiceProviderContext {
    public override ProvideHandlers Invoke(Invocation<M, R, O, C> invocation, ProvideHandlers phase) {
        IEnumerable<IHandles<M>>? handlers = (IEnumerable<IHandles<M>>)invocation
            .Context.OperationServices
            .GetService(typeof(IEnumerable<IHandles<M>>));

        if (handlers == null) return phase;

        return phase with {
            Handlers = handlers is IHandles<M>[] hs ?
                hs :
                handlers.ToArray()
        };
    }

    public override ValueTask<ProvideHandlers> InvokeAsync(Invocation<M, R, O, C> invocation, ProvideHandlers hs) {
        return new ValueTask<ProvideHandlers>(Invoke(invocation, hs));
    }
}
