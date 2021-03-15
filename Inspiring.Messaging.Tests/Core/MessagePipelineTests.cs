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

            WHEN["sending a message"] |= () => result = InvokePipeline(new TestMessage(), mws);
            THEN["the aggregated results are returned"] |= () => result.Should().BeEquivalentTo("[Result 2]", "[Result 1]");
        }

        [Scenario]
        internal void GenericMiddleware(ProcessorMock<IMessage, object> mw, ProcessorMock<OtherMessage, object> nonApplicableMw) {
            GIVEN["some generic middlewares"] |= () => {
                mw = new ProcessorMock<IMessage, object>();
                nonApplicableMw = new ProcessorMock<OtherMessage, object>();
            };
            WHEN["sending a message"] |= () => InvokePipeline(new TestMessage(), nonApplicableMw, mw);
            THEN["compatible middlewares are called"] |= () => mw.WasCalled.Should().BeTrue();
            AND["incompatible middlewares are not called"] |= () => nonApplicableMw.WasCalled.Should().BeFalse();
        }

        private R InvokePipeline<M, R>(IMessage<M, R> message, params IMessageMiddleware[] middlewares) where M : IMessage<M, R> {
            MessagePipeline<M, R> p = new(
                middlewares,
                (IResultAggregator<R>)new TestAggregator());

            return p.Process((M)message, new MessageContext(_container));
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

        public class TestMessage : IMessage<TestMessage, string[]> { }

        public class OtherMessage : IMessage<OtherMessage, string[]> { }


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
