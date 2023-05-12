using System;
using System.Collections.Generic;
using System.Text;

namespace Inspiring.Messaging.Pipelines {
    public interface IGenericStepFactory<TPhase> {
        PipelineStep<M, R, O, C, TPhase> Create<M, R, O, C>(PipelineStep<M, R, O, C, TPhase> next);
    }
}
