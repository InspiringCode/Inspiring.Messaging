using System;
using System.Collections.Generic;
using System.Text;

namespace Inspiring.Messaging.Pipelines {
    public interface IPipelineBuilder {
        Type MessageType { get; }

        Type ResultType { get; }

        Type OperationType { get; }

        Type ContextType { get; }

        IEnumerable<IStepInfo> Steps { get; } 

        void AddStep<TPhase>(IStepFactory<TPhase> stepFactory, string factoryMethodName, int order = 0, object? tag = null);

        void AddStep<TPhase>(IGenericStepFactory<TPhase> stepFactory, int order = 0, object? tag = null);
    }
}
