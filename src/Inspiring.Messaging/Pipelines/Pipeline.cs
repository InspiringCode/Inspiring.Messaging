using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Inspiring.Messaging.Pipelines {
    public class Pipeline<M, R, O, C> {
        private readonly object[] _rootSteps;

        internal Pipeline(object[] rootSteps)
            => _rootSteps = rootSteps;

        protected Pipeline() : this(Array.Empty<object>()) { }

        public virtual TPhase Invoke<TPhase>(Invocation<M, R, O, C> invocation, TPhase phase)
            => FindPipelinePhase<TPhase>().Invoke(invocation, phase);

        public virtual ValueTask<TPhase> InvokeAsync<TPhase>(Invocation<M, R, O, C> invocation, TPhase phase)
            => FindPipelinePhase<TPhase>().InvokeAsync(invocation, phase);

        private PipelineStep<M, R, O, C, TPhase> FindPipelinePhase<TPhase>() {
            foreach (object rootStep in _rootSteps) {
                if (rootStep is PipelineStep<M, R, O, C, TPhase> p)
                    return p;
            }

            throw new ArgumentException(
                $"No phase of type '{typeof(TPhase).Name}' has been registered for this pipeline. Make sure " +
                $"that you invoke the pipeline with the correct generic arguments or make sure that at least " +
                $"one message behavior adds a pipeline step for the given phase type.");
        }
    }
}
