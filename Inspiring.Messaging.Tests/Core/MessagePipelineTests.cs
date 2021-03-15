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
        internal void Process(TestHandler[] handlers, string[] result) {
            GIVEN["some registered handlers"] |= () => handlers = new[] {
                new TestHandler { Input = "Result 1" },
                new TestHandler { Input = "Result 2" } };

            WHEN["sending a message"] |= () => result = Send(new TestMessage(), handlers: handlers, aggregator: new TestAggregator());
            THEN["the aggregated results are returned"] |= () => result.Should().BeEquivalentTo("Result 1", "Result 2");
        }

        [Scenario]
        internal void Middlewares(IMessageMiddleware[] mws, string[] result) {
            GIVEN["several kinds of middleware"] |= () => {
                var handlerProvider = Substitute.For<IHandlerProvider<TestMessage, string[]>>();
                handlerProvider.GetHandlers<TestMessage, string[]>(default, default, default).ReturnsForAnyArgs(new[] {
                    new TestHandler { Input = "Result 1" },
                    new TestHandler { Input = "Result 2" },
                    new TestHandler { Input = "Result 3" }
                });

                mws = new IMessageMiddleware[] {
                    handlerProvider,
                    new TestMessageAggregator(),
                    new DispatcherAndInvoker() };
            };

            WHEN["sending a message"] |= () => result = Send(new TestMessage(), mws, aggregator: new TestAggregator());
            THEN["the aggregated results are returned"] |= () => result.Should().BeEquivalentTo("[Result 2]", "[Result 1]");
        }

        [Scenario]
        internal void GenericMiddleware(ProcessorMock<IMessage, object> mw, ProcessorMock<OtherMessage, object> nonApplicableMw) {
            GIVEN["some generic middlewares"] |= () => {
                mw = new ProcessorMock<IMessage, object>();
                nonApplicableMw = new ProcessorMock<OtherMessage, object>();
            };
            WHEN["sending a message"] |= () => Send(new TestMessage(), new IMessageMiddleware[] { nonApplicableMw, mw });
            THEN["compatible middlewares are called"] |= () => mw.WasCalled.Should().BeTrue();
            AND["incompatible middlewares are not called"] |= () => nonApplicableMw.WasCalled.Should().BeFalse();
        }

        [Scenario]
        internal void DefaultPipelineBehavior(IHandles<TestMessage<string>, string>[] handlers, string result) {
            GIVEN["multiple handlers"] |= () => handlers = new[] {
                new TestHandler<TestMessage<string>, string>() { Result = "R1" },
                new TestHandler<TestMessage<string>, string>() { Result = "R2" },
            };
            WHEN["sending a message"] |= () => result = Send(new TestMessage<string>(), handlers: handlers);
            THEN["the last result is returned"] |= () => result.Should().Be("R2");
        }

        private R Send<M, R>(
            IMessage<M, R> message, 
            IMessageMiddleware[] middlewares = null,
            IHandles<M,R>[] handlers = null,
            IResultAggregator<R> aggregator = null
        ) where M : IMessage<M, R> {
            TestContainer c = new();

            if (aggregator != null)
                c.Add(new MessagePipeline<M, R>(middlewares ?? Enumerable.Empty<IMessageMiddleware>(), aggregator));
            else if (middlewares != null)
                c.Add(new MessagePipeline<M, R>(middlewares));

            if (handlers != null)
                c.Add(handlers);

            return new ContainerMessenger(c).Send(message);
        }

        internal class ProcessorMock<MBase, RBase> : IMessageProcessor<MBase, RBase> {
            public bool WasCalled { get; set; }

            public R Process<M, R>(M m, MessageContext context, Func<M, MessageContext, R> next)
                where M : MBase, IMessage<M, R>
                where R : RBase {

                WasCalled = true;
                return next(m, context);
            }
        }

        internal class TestMessageAggregator : IMessageResultAggregator<TestMessage, string[]> {
            public string[] Aggregate<M>(
                M m,
                MessageContext context,
                IEnumerable<string[]> results,
                Func<M, MessageContext, IEnumerable<string[]>, string[]> next
            ) where M : TestMessage, IMessage<M, string[]> {
                return next(m, context, results)
                    .Reverse()
                    .ToArray();
            }
        }

        internal class DispatcherAndInvoker :
            IMessageDispatcher<TestMessage, IEnumerable<string>>,
            IHandlerInvoker<TestMessage, IEnumerable<string>> {

            public IEnumerable<R> Dispatch<M, R>(
                M m,
                MessageContext context,
                IEnumerable<IHandles<M, R>> handlers,
                Func<M, MessageContext, IEnumerable<IHandles<M, R>>, IEnumerable<R>> next
            ) where M : TestMessage, IMessage<M, R> where R : IEnumerable<string> {
                return next(m, context, handlers.Take(2));
            }

            public R Invoke<M, R>(
                M m,
                MessageContext context,
                IHandles<M, R> h,
                Func<M, MessageContext, IHandles<M, R>, R> next
            ) where M : TestMessage, IMessage<M, R> where R : IEnumerable<string> {
                return (R)(object)h
                    .Handle(m)
                    .Select(x => $"[{x}]")
                    .ToArray();
            }
        }

        public class TestMessage<R> : IMessage<TestMessage<R>, R> { }

        public class TestMessage : IMessage<TestMessage, string[]> { }

        public class OtherMessage : IMessage<OtherMessage, string[]> { }


        internal class TestHandler : IHandles<TestMessage, string[]> {
            public string Input { get; set; }

            public string[] Handle(TestMessage m)
                => new[] { Input };
        }

        internal class TestHandler<M, R> : IHandles<M, R> where M : IMessage<M, R> {
            public R Result { get; set; }

            public R Handle(M m)
                => Result;
        }

        internal class TestAggregator : IResultAggregator<string[]> {
            public string[] Aggregate(IEnumerable<string[]> values) => values
                .SelectMany(x => x)
                .ToArray();
        }
    }
}
