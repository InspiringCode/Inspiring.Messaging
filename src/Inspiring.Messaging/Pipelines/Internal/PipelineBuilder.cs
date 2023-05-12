using Inspiring.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Inspiring.Messaging.Pipelines.Internal {
    internal class PipelineBuilder<M, R, O, C> : IPipelineBuilder {
        private readonly List<PhaseBuilder> _phases = new();

        public Type MessageType => typeof(M);

        public Type ResultType => typeof(R);

        public Type OperationType => typeof(O);

        public Type ContextType => typeof(C);

        public IEnumerable<IStepInfo> Steps => _phases.SelectMany(p => p.Steps);

        public void AddStep<TPhase>(IStepFactory<TPhase> stepFactory, string factoryMethodName, int order = 0, object? tag = null) {
            MethodInfo factoryMethod = stepFactory
                .GetType()
                .GetMethod(factoryMethodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance) ??
                    throw createArgumentException(
                        $"The given step factory instance does not define any instance method with the specified " +
                        $"name ('{factoryMethodName}').",
                        nameof(factoryMethodName));

            bool hasExpectedSignature =
                factoryMethod.ReturnParameter.ParameterType.Name == typeof(PipelineStep<,,,,>).Name &&
                factoryMethod.GetParameters() is { Length: 1 } ps &&
                ps[0].ParameterType.Name == typeof(PipelineStep<,,,,>).Name;

            if (!hasExpectedSignature)
                throw createArgumentException("The specified factory method has the wrong method signature.", nameof(stepFactory));

            if (factoryMethod.TryInferTypes(new[] { typeof(PipelineStep<M, R, O, C, TPhase>) }, out Type[] genericArgs)) {

                Delegate factoryDelegate = factoryMethod
                    .MakeGenericMethod(genericArgs)
                    .CreateDelegate(typeof(Func<PipelineStep<M, R, O, C, TPhase>, PipelineStep<M, R, O, C, TPhase>>), stepFactory);

                AddStep(new DelegateStepFactory<TPhase>(factoryDelegate), order, tag);
            }

            ArgumentException createArgumentException(string message, string argumentName) =>
                new ArgumentException(
                    $"{message} Make sure that the given step factory contains exactly one method 'PipelineStep<...> " +
                    $"{factoryMethodName}<...>(PipelineStep<...> next)' where the generic arguments of PipelineStep " +
                    $"may be concrete types, generic parameters (with or without constraints) or generic types that " +
                    $"contain generic parameters.", argumentName);
        }


        public void AddStep<TPhase>(IGenericStepFactory<TPhase> stepFactory, int order = 0, object? tag = null) {
            PhaseBuilder<TPhase>? phase = _phases
                .OfType<PhaseBuilder<TPhase>>()
                .FirstOrDefault();

            if (phase == null)
                _phases.Add(phase = new PhaseBuilder<TPhase>());

            phase.AddStep(stepFactory, order, tag);
        }

        public void SetTerminalStep<TPhase>(PipelineStep<M, R, O, C, TPhase> step) {
            PhaseBuilder<TPhase>? phase = _phases
                .OfType<PhaseBuilder<TPhase>>()
                .FirstOrDefault();

            if (phase == null)
                _phases.Add(phase = new PhaseBuilder<TPhase>());

            phase.TerminationStep = step;
        }

        public Pipeline<M, R, O, C> Build() {
            return new Pipeline<M, R, O, C>(_phases
                .Select(p => p.Build())
                .ToArray());
        }

        private class DelegateStepFactory<TPhase> : IGenericStepFactory<TPhase> {
            private readonly Delegate _factory;

            public DelegateStepFactory(Delegate factory)
                => _factory = factory;

            public PipelineStep<M1, R1, O1, C1, TPhase> Create<M1, R1, O1, C1>(PipelineStep<M1, R1, O1, C1, TPhase> next)
                => ((Func<PipelineStep<M1, R1, O1, C1, TPhase>, PipelineStep<M1, R1, O1, C1, TPhase>>)_factory)(next);
        }

        private abstract class PhaseBuilder {
            public abstract IEnumerable<IStepInfo> Steps { get; }

            public abstract object Build();
        }

        private class PhaseBuilder<TPhase> : PhaseBuilder {
            private readonly List<StepInfo> _steps = new();

            public PipelineStep<M, R, O, C, TPhase>? TerminationStep { get; set; }

            public override IEnumerable<IStepInfo> Steps => _steps;

            public void AddStep(IGenericStepFactory<TPhase> factory, int order, object? tag)
                => _steps.Add(new StepInfo { Factory = factory, Order = order, Tag = tag });

            public override object Build() {
                PipelineStep<M, R, O, C, TPhase> root = TerminationStep ?? CreateDefaultTerminationStep();
                foreach (var step in _steps.OrderByDescending(s => s.Order))
                    root = step.Factory.Create(next: root);
                return root;
            }

            // We keep this call in a separate method in the hope that the JIT does not compile it
            // unless need and that we can avoid creating another generic type of NoOpStep unless we
            // really need it.
            [MethodImpl(MethodImplOptions.NoInlining)]
            private PipelineStep<M, R, O, C, TPhase> CreateDefaultTerminationStep() {
                return NoOpStep<M, R, O, C, TPhase>.Default;
            }

            private class StepInfo : IStepInfo {
                public required IGenericStepFactory<TPhase> Factory { get; init; }

                public int Order { get; init; }

                public object? Tag { get; init; }
            }
        }
    }
}
