using System;
using System.Collections.Generic;
using System.Linq;

namespace Inspiring.Messaging.Pipelines {
    public class PipelineFactoryTests : FeatureBase {
        [Scenario]
        internal void Test(
            TestPipelineFactory f,
            IEnumerable<IStepInfo> steps,
            Action action
        ) {
            GIVEN["a factory with injected behaviors and a behavior provider"] = () => f = new TestPipelineFactory(
                new[] {
                    new TestBehavior<object>(tag: "B1"),
                    new TestBehavior<object>(tag: "B2") },
                new TestServiceProvider {
                    new TestBehavior<TestResult>(tag: "B3"),
                    new TestBehavior<TestMessage>(tag: "B4"),
                    new TestBehavior<ITestMessage>(tag: "B5")
                    // context and operation behavior are constructed via reflection
                });
            WHEN["creating the pipeline"] = () => steps = configurePipeline<TestMessage, TestResult, object, object>(f);
            THEN["the injected behaviors and the behaviors on the message, result, operation and context type are configured"] = () => steps
                .Select(x => x.Tag)
                .Should().BeEquivalentTo(
                    new object[] { "B1", "B2", "B3", typeof(ITestResult), "B4", "B5" },
                    o => o.WithStrictOrdering());

            GIVEN["a factory without a behavior provider"] = () => f = new TestPipelineFactory();
            WHEN["creating the pipeline"] = () => steps = configurePipeline<TestMessage, TestResult, TestOperation, TestContext>(f);
            THEN["the configured beaviors are created via reflection"] = () => steps
                .Select(x => x.Tag).Should().BeEquivalentTo(new[] {
                    typeof(TestResult), typeof(ITestResult),
                    typeof(TestMessage), typeof(ITestMessage),
                    typeof(TestContext), typeof(ITestContext),
                    typeof(TestOperation), typeof(ITestOperation)
                });

            //WHEN["a behavior is defined on an interface"] = () => steps = configurePipeline<MessageWithInterfaceBehavior, object, object, object>();
            //THEN["it is still considered"] = steps.

            WHEN["a non-resolvable behavior does not provide a parameterless ctor"] = () => action =
                new Action(() => configurePipeline<NoParameterlessCtorMessage, TestResult, TestOperation, TestContext>());
            THEN["a helpful exception is thrown"] = () => action
                .Should().Throw<ArgumentException>()
                .Which.Message.Should().Contain("IServiceProvider");

            IEnumerable<IStepInfo> configurePipeline<M, R, O, C>(TestPipelineFactory? f = null) {
                f ??= new();
                PipelineBuilder<M, R, O, C> b = new();
                f.ConfigurePipeline(b);
                return b.Steps;
            }
        }

        internal class TestPipelineFactory : PipelineFactory {
            public TestPipelineFactory() { }

            public TestPipelineFactory(IEnumerable<IMessageBehavior> generalBehaviors)
                : base(generalBehaviors) { }

            public TestPipelineFactory(IEnumerable<IMessageBehavior> generalBehaviors, IServiceProvider behaviorFactory)
                : base(generalBehaviors, behaviorFactory) { }

            public new void ConfigurePipeline<M, R, O, C>(PipelineBuilder<M, R, O, C> b)
                => base.ConfigurePipeline(b);
        }

        [MessageBehavior(typeof(TestBehavior<TestMessage>))]
        [MessageBehavior(typeof(TestBehavior<ITestMessage>))] // Should be considered only once
        internal class TestMessage : ITestMessage { }

        [MessageBehavior(typeof(TestBehavior<ITestMessage>))]
        internal interface ITestMessage { }

        [MessageBehavior(typeof(NoParameterlessCtorBehavior))]
        internal class NoParameterlessCtorMessage { }

        [MessageBehavior(typeof(TestBehavior<TestOperation>))]
        internal struct TestOperation : ITestOperation { }

        [MessageBehavior(typeof(TestBehavior<TestContext>))]
        internal struct TestContext : ITestContext { }

        [MessageBehavior(typeof(TestBehavior<TestResult>))]
        internal struct TestResult : ITestResult { }

        internal struct TestPhase { }


        [MessageBehavior(typeof(TestBehavior<ITestOperation>))]
        internal interface ITestOperation { }

        [MessageBehavior(typeof(TestBehavior<ITestContext>))]
        internal interface ITestContext { }

        [MessageBehavior(typeof(TestBehavior<ITestResult>))]
        internal interface ITestResult { }

        internal class TestBehavior<T> : IMessageBehavior {
            private readonly object _tag;

            private TestBehavior() { }

            public TestBehavior(object tag) => _tag = tag;

            public void Configure<M, R, O, C>(PipelineBuilder<M, R, O, C> pipeline) {
                pipeline.AddStep<TestPhase>(next => next, tag: _tag ?? typeof(T));
            }
        }

        internal class NoParameterlessCtorBehavior {
            public NoParameterlessCtorBehavior(object arg) { }
        }
    }
}
