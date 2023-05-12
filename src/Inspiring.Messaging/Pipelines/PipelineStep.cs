using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Inspiring.Messaging.Pipelines {
    public abstract class PipelineStep<M, R, O, C, P> {
        public abstract P Invoke(Invocation<M, R, O, C> i, P phase);

        public abstract ValueTask<P> InvokeAsync(Invocation<M, R, O, C> i, P phase);
    }
}
