using Inspiring.Messaging.Pipelines;
using System;
using System.Threading.Tasks;

namespace Inspiring.Messaging {
    internal class DelegateStep<M, R, O, C, TPhase> : PipelineStep<M, R, O, C, TPhase> {
        private readonly PipelineStep<M, R, O, C, TPhase> _next;
        private readonly Action _action;

        public DelegateStep(PipelineStep<M, R, O, C, TPhase> next, Action action = null)
            => (_next, _action) = (next, action ?? delegate { });

        public override TPhase Invoke(Invocation<M, R, O, C> invocation, TPhase phase) {
            _action();
            return _next.Invoke(invocation, phase);
        }

        public override ValueTask<TPhase> InvokeAsync(Invocation<M, R, O, C> invocation, TPhase phase) {
            _action();
            return _next.InvokeAsync(invocation, phase);
        }
    }
}
