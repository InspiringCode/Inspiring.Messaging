using Inspiring.Messaging.Pipelines.Internal;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inspiring.Messaging.Pipelines {
    public class PipelineFactoryTests : FeatureBase {
        [Scenario]
        internal void Test(
            TestPipelineFactory f, 
            PipelineBuilder<TestMessage, TestResult, TestOperation, TestContext> builder,
            Action action
        ) {
            GIVEN["a factory with injected behaviors and a behavior provider"] = () => f = new TestPipelineFactory(
                new[] {
                    new TestBehavior<object>(tag: "B1"),
                    new TestBehavior<object>(tag: "B2") },
                new TestServiceProvider {
                    new TestBehavior<TestResult>(tag: "B3"),
                    new TestBehavior<TestMessage>(tag: "B4")
                    // context and operation behavior are constructed via reflection
                });
            WHEN["creating the pipeline"] = () => f.ConfigurePipeline(builder = new());
            THEN["the injected behaviors and the behaviors on the message, result, operation and context type are configured"] = () => builder
                .Steps.Select(x => x.Tag)
                .Should().BeEquivalentTo(
                    new object[] { "B1", "B2", "B3", "B4", typeof(TestContext), typeof(TestOperation) },
                    o => o.WithStrictOrdering());

            GIVEN["a factory without a behavior provider"] = () => f = new TestPipelineFactory();
            WHEN["creating the pipeline"] = () => f.ConfigurePipeline(builder = new());
            THEN["the configured beaviors are created via reflection"] = () => builder
                .Steps.Select(x => x.Tag)
                .Should().BeEquivalentTo(new[] { typeof(TestResult), typeof(TestMessage), typeof(TestContext), typeof(TestOperation) });

            WHEN["a non-resolvable behavior does not provide a paramterless ctor"] = () => action =
                new Action(() => f.ConfigurePipeline(new PipelineBuilder<NoParameterlessCtorMessage, TestResult, TestOperation, TestContext>()));
            THEN["a helpful exception is thrown"] = () => action
                .Should().Throw<ArgumentException>()
                .Which.Message.Should().Contain("IServiceProvider");            
        }

        internal class TestPipelineFactory : PipelineFactory {
            public TestPipelineFactory() { }

            public TestPipelineFactory(IEnumerable<IMessageBehavior> generalBehaviors)
                : base(generalBehaviors) { }

            public TestPipelineFactory(IEnumerable<IMessageBehavior> generalBehaviors, IServiceProvider behaviorFactory)
                : base(generalBehaviors, behaviorFactory) { }

            public void ConfigurePipeline<M, R, O, C>(PipelineBuilder<M, R, O, C> b)
                => base.ConfigurePipeline(b);
        }

        [MessageBehavior(typeof(TestBehavior<TestMessage>))]
        internal class TestMessage { }

        [MessageBehavior(typeof(NoParameterlessCtorBehavior))]
        internal class NoParameterlessCtorMessage { }

        [MessageBehavior(typeof(TestBehavior<TestOperation>))]
        internal struct TestOperation { }

        [MessageBehavior(typeof(TestBehavior<TestContext>))]
        internal struct TestContext { }

        [MessageBehavior(typeof(TestBehavior<TestResult>))]
        internal struct TestResult { }

        internal struct TestPhase { }

        internal class TestBehavior<T> : IMessageBehavior, IGenericStepFactory<TestPhase> {
            private readonly object _tag;

            private TestBehavior() { }

            public TestBehavior(object tag) => _tag = tag;

            public virtual void Configure(IPipelineBuilder pipeline) {
                pipeline.AddStep(this, tag: _tag ?? typeof(T));
            }

            public PipelineStep<M, R, O, C, TestPhase> Create<M, R, O, C>(PipelineStep<M, R, O, C, TestPhase> next) => next;
        }

        internal class NoParameterlessCtorBehavior {
            public NoParameterlessCtorBehavior(object arg) { }
        }
    }
}
