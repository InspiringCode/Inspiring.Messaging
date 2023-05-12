using System;
using System.Collections.Generic;
using System.Text;

namespace Inspiring.Messaging.Pipelines.Internal {
    internal class ReflectionStepFactory<TPhase> : IGenericStepFactory<TPhase> {

        //public static IGenericStepFactory<TPhase> CreateGenericFactory(IStepFactory<TPhase> factory, string factoryMethodName)
        
        public PipelineStep<M, R, O, C, TPhase> Create<M, R, O, C>(PipelineStep<M, R, O, C, TPhase> next) {
            throw new NotImplementedException();
        }
    }
}
