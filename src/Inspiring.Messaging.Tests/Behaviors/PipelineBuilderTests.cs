using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Inspiring.Messaging.Pipelines {
    public class PipelineBuilderTests : FeatureBase {
        [Scenario]
        internal void Ordering(
            PipelineBuilder<TestMessage, int, object, object> b,
            Pipeline<TestMessage, int, object, object> p,
            string actualSequence
        ) {
            GIVEN["a pipeline with same steps and phases"] = () => b = new();
            WHEN["building and invoking the pipeline"] = () => {
                actualSequence = "";
                b.AddStep<Phase1>(new DelegateStepFactory<Phase1>(() => { }).Create);
                b.AddStep<Phase2>(new DelegateStepFactory<Phase2>(() => actualSequence += "2 ").Create, order: 1, tag: "A");
                b.AddStep<Phase2>(new DelegateStepFactory<Phase2>(() => actualSequence += "3 ").Create, order: 5, tag: "B");
                b.AddStep<Phase2>(new DelegateStepFactory<Phase2>(() => actualSequence += "1 ").Create, order: 1, tag: "C");
                p = b.Build();
                p.Invoke(new Invocation<TestMessage, int, object, object>(), new Phase2());
                p.Invoke(new Invocation<TestMessage, int, object, object>(), new Phase1());
            };

            THEN["the order of adding is preserved for steps with the same specified order"] = ()
                => actualSequence.Should().Be("1 2 3 ");
            AND["the metadata of the steps (order, tag) can be queried"] = () => b.Steps.Should().BeEquivalentTo(new[] {
                new { Tag = "A", Order = 1 },
                new { Tag = "B", Order = 5 },
                new { Tag = "C", Order = 1 },
                new { Tag = (string)null, Order = 0 }
            });
        }

        [Scenario]
        internal void ReflectionSteps(ReflectionStepFactory sf, PipelineBuilder<TestMessage, int, object, object> b, Action action) {
            GIVEN["a reflection based step factory"] = () => sf = new();
            WHEN["adding the steps to an incompatible builder"] = () => {
                b = new();
                b.AddStep<Phase1>(new DelegateStepFactory<Phase1>(() => { }).Create);
                b.AddStep(sf, "CreateStepForAsyncStringMessage");
                b.AddStep(sf, "CreateStepForTestInterfaceContext");
                b.Build().Invoke(new(), new Phase1());
            };
            THEN["the steps are not added to the pipeline"] = () => sf.Invocations.Should().BeEmpty();

            WHEN["adding the steps to a compatible builder"] = () => {
                PipelineBuilder<TestMessage, Task<string>, object, TestContext> b = new();
                b.AddStep(sf, "CreateStepForAsyncStringMessage");
                b.AddStep(sf, "CreateStepForTestInterfaceContext");
                b.Build().Invoke(new(), new Phase1());
            };
            THEN["the steps are executed"] = () => sf.Invocations.Should().BeEquivalentTo("context-step", "async-string-step");

            WHEN["adding a step where the method is not found"] = () => action = new Action(() => b.AddStep(sf, "NonexistingMethod"));
            THEN["an exception is thrown"] = () => action.Should().Throw<ArgumentException>().Which.Message.Should().Match("*not define*");

            WHEN["adding a step where the method has an unexpected signature"] = () => action = new Action(() => b.AddStep(sf, "InvalidSignature"));
            THEN["an exception is thrown"] = () => action.Should().Throw<ArgumentException>().Which.Message.Should().Match("*wrong*signature*");
        }

        internal class ReflectionStepFactory : IStepFactory<Phase1> {
            public List<string> Invocations { get; } = new();

            public PipelineStep<M, Task<string>, O, C, Phase1> CreateStepForAsyncStringMessage<M, O, C>(
                PipelineStep<M, Task<string>, O, C, Phase1> next
            ) {
                return new DelegateStep<M, Task<string>, O, C, Phase1>(next, () => Invocations.Add("async-string-step"));
            }

            private PipelineStep<M, R, O, C, Phase1> CreateStepForTestInterfaceContext<M, O, C, R>(
                PipelineStep<M, R, O, C, Phase1> next
            ) where C : ITestContext {
                return new DelegateStep<M, R, O, C, Phase1>(next, () => Invocations.Add("context-step"));
            }

            private Invocation<M, R, O, C> InvalidSignature<M, O, C, R>(
                Invocation<M, R, O, C> next
            ) where C : ITestContext {
                return new();
            }
        }

        private interface ITestContext { }

        private class TestContext : ITestContext { }


        private class DelegateStepFactory<TPhase> : IGenericStepFactory<TPhase> {
            //private readonly Func<DelegateInvocation<TPhase>, TPhase> _func;
            private readonly Action _action;

            public DelegateStepFactory(Action action) {
                _action = action;
            }

            public PipelineStep<M, R, O, C, TPhase> Create<M, R, O, C>(PipelineStep<M, R, O, C, TPhase> next) {
                return new DelegateStep<M, R, O, C, TPhase>(next, _action);
            }

        }

        //private record DelegateInvocation<TPhase>(object Message, object Operation, object Context, TPhase Phase);

        internal class TestMessage { }

        internal class Phase1 { }

        internal class Phase2 { }
    }
}
