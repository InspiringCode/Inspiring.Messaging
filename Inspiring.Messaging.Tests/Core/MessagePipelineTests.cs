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
        private TestContainer _container;

        [Background]
        public void Background() {
            USING["a test container"] |= () => _container = new TestContainer();
        }

        [Scenario]
        internal void Process(string[] result) {
            GIVEN["some registered handlers"] |= () => _container.Add(new[] {
                new TestHandler { Input = "Result 1" },
                new TestHandler { Input = "Result 2" } });

            WHEN["sending a message"] |= () => result = InvokePipeline(new TestMessage());

            THEN["the aggregated results are returned"] |= () => result.Should().BeEquivalentTo("Result 1", "Result 2");
        }

        [Scenario]
        internal void Middlewares(IMessageMiddleware[] mws, string[] result) {
            GIVEN["several kinds of middleware"] |= () => {
                var handlerProvider = Substitute.For<IHandlerProvider<TestMessage, string[]>>();
                handlerProvider.GetHandlers(default, default, default).ReturnsForAnyArgs(new[] {
                    new TestHandler { Input = "Result 1" },
                    new TestHandler { Input = "Result 2" },
                    new TestHandler { Input = "Result 3" }
                });

                mws = new IMessageMiddleware[] {
                    handlerProvider,
                    new TestMessageAggregator(),
                    new DispatcherAndInvoker() };
            };

            WHEN["sending a message"] |= () => result = InvokePipeline(new TestMessage(), mws);
            THEN["the aggregated results are returned"] |= () => result.Should().BeEquivalentTo("[Result 2]", "[Result 1]");
        }

        private R InvokePipeline<M, R>(IMessage<M, R> message, params IMessageMiddleware[] middlewares) where M : IMessage<M, R> {
            MessagePipeline<M, R> p = new(
                middlewares,
                (IResultAggregator<R>)new TestAggregator());

            return p.Process((M)message, new MessageContext(_container));
        }

        internal class TestMessageAggregator : IMessageResultAggregator<TestMessage, string[]> {
            public string[] Aggregate(
                TestMessage m,
                MessageContext context,
                IEnumerable<string[]> results,
                Func<TestMessage, MessageContext, IEnumerable<string[]>, string[]> next
            ) {
                return next(m, context, results)
                    .Reverse()
                    .ToArray();
            }
        }

        internal class DispatcherAndInvoker :
            IMessageDispatcher<TestMessage, string[]>,
            IHandlerInvoker<TestMessage, string[]> {

            public IEnumerable<string[]> Dispatch(
                TestMessage m,
                MessageContext context,
                IEnumerable<IHandles<TestMessage, string[]>> handlers,
                Func<TestMessage, MessageContext, IEnumerable<IHandles<TestMessage, string[]>>, IEnumerable<string[]>> next
            ) {
                return next(m, context, handlers.Take(2));
            }

            public string[] Invoke(
                TestMessage m,
                MessageContext context,
                IHandles<TestMessage, string[]> h,
                Func<TestMessage, MessageContext, IHandles<TestMessage, string[]>, string[]> next
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
