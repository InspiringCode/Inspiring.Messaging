using FluentAssertions;
using Inspiring.Messaging.Core;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using Xbehave;

namespace Inspiring.Messaging.Tests.Core {
    // TODO: Make all these tests better and clearer...
    public class MessagePipelineTests : Feature {
        [Scenario]
        internal void Process(MessagePipeline<TestMessage, string[]> p, string[] result) {
            GIVEN["a message pipeline"] |= () => p = new MessagePipeline<TestMessage, string[]>(
                Enumerable.Empty<IMessageMiddleware<TestMessage, string[]>>(),
                () => new[] {
                    new TestHandler { Input = "Result 1" },
                    new TestHandler { Input = "Result 2" }
                },
                () => new TestAggregator());

            WHEN["sending a message"] |= () => result = p.Process(new TestMessage(), new PipelineParameters());

            THEN["the aggregated results are returned"] |= () => result.Should().BeEquivalentTo("Result 1", "Result 2");
        }

        [Scenario]
        internal void Middlewares(MessagePipeline<TestMessage, string[]> p, string[] result) {
            GIVEN["a message pipeline"] |= () => {
                var handlerProvider = Substitute.For<IHandlerProvider<TestMessage, string[]>>();
                handlerProvider.GetHandlers(default, default, default).ReturnsForAnyArgs(new[] {
                    new TestHandler { Input = "Result 1" },
                    new TestHandler { Input = "Result 2" },
                    new TestHandler { Input = "Result 3" }
                });

                p = new MessagePipeline<TestMessage, string[]>(
                    new IMessageMiddleware<TestMessage, string[]>[] {
                        handlerProvider,
                        new TestMessageAggregator(),
                        new DispatcherAndInvoker()
                    },
                    () => throw new InvalidOperationException(),
                    () => new TestAggregator());
            };

            WHEN["sending a message"] |= () => result = p.Process(new TestMessage(), new PipelineParameters());

            THEN["the aggregated results are returned"] |= () => result.Should().BeEquivalentTo("[Result 2]", "[Result 1]");
        }

        internal class TestMessageAggregator : IMessageResultAggregator<TestMessage, string[]> {
            public string[] Aggregate(
                TestMessage m,
                PipelineParameters ps,
                IEnumerable<string[]> results,
                Func<TestMessage, PipelineParameters, IEnumerable<string[]>, string[]> next
            ) {
                return next(m, ps, results)
                    .Reverse()
                    .ToArray();
            }
        }

        internal class DispatcherAndInvoker :
            IMessageDispatcher<TestMessage, string[]>,
            IHandlerInvoker<TestMessage, string[]> {

            public IEnumerable<string[]> Dispatch(
                TestMessage m,
                PipelineParameters ps,
                IEnumerable<IHandles<TestMessage, string[]>> handlers,
                Func<TestMessage, PipelineParameters, IEnumerable<IHandles<TestMessage, string[]>>, IEnumerable<string[]>> next
            ) {
                return next(m, ps, handlers.Take(2));
            }

            public string[] Invoke(
                TestMessage m,
                PipelineParameters ps,
                IHandles<TestMessage, string[]> h,
                Func<TestMessage, PipelineParameters, IHandles<TestMessage, string[]>, string[]> next
            ) {
                return h
                    .Handle(m)
                    .Select(x => $"[{x}]")
                    .ToArray();
            }
        }

        public class TestMessage : IMessage<TestMessage, string[]> { }

        internal class TestHandler : IHandles<TestMessage, string[]> {
            public string Input { get; set; }

            public string[] Handle(TestMessage m)
                => new[] { Input };
        }

        internal class TestAggregator : IResultAggregator<string[]> {
            public string[] Aggregate(IEnumerable<string[]> values) => values
                .SelectMany(x => x)
                .ToArray();
        }
    }
}
