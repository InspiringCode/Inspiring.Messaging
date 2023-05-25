using Inspiring.Messaging.Pipelines;
using System;
using System.Threading.Tasks;

namespace Inspiring.Messaging {
    internal class DelegateBehavior<M, R, O, C, TPhase> : IMessageBehavior {
        private readonly Action<Invocation<M, R, O, C>, TPhase> _action;
        private readonly int _order;

        public DelegateBehavior(Action<Invocation<M, R, O, C>, TPhase> action, int order = int.MinValue) {
            _action = action;
            _order = order;
        }

        public void Configure<M1, R1, O1, C1>(PipelineBuilder<M1, R1, O1, C1> pipeline) {
            if (pipeline is PipelineBuilder<M, R, O, C> p)
                p.AddStep<TPhase>(next => new DelegateStep(next, _action), order: _order);
        }

        internal class DelegateStep : PipelineStep<M, R, O, C, TPhase> {
            private readonly PipelineStep<M, R, O, C, TPhase> _next;
            private readonly Action<Invocation<M, R, O, C>, TPhase> _action;

            public DelegateStep(PipelineStep<M, R, O, C, TPhase> next, Action<Invocation<M, R, O, C>, TPhase> action)
                => (_next, _action) = (next, action);

            public override TPhase Invoke(Invocation<M, R, O, C> invocation, TPhase phase) {
                TPhase p = _next.Invoke(invocation, phase);
                _action(invocation, p);
                return p;
            }

            public override async ValueTask<TPhase> InvokeAsync(Invocation<M, R, O, C> invocation, TPhase phase) {
                TPhase p = await _next.InvokeAsync(invocation, phase);
                _action(invocation, p);
                return p;
            }
        }
    }
}
