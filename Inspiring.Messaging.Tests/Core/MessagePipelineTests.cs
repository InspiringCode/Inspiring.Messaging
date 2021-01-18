using Inspiring.Messaging.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Xbehave;

namespace Inspiring.Messaging.Tests.Core {
    public class MessagePipelineTests : Feature {
        [Scenario]
        internal void Process(MessagePipeline<TestMessage, string[]> p, string[] result) {
            GIVEN["a message pipeline"] |= () => p = new MessagePipeline<TestMessage, string[]>(
                Enumerable.Empty<IMessageMiddleware<TestMessage, string[]>>(),
                () => new[] {
                    new TestHandler { Input = "1" },
                    new TestHandler { Input = "2" }
                },
                () => new TestAggregator());

            WHEN["sending a message"] |= () => result = p.Process(new TestMessage(), new PipelineParameters());

            THEN["the aggregated results are returned"] |= () => result.Should().BeEquivalentTo("1", "2");
        }

        internal class TestMessage : IMessage<TestMessage, string[]> { }

        internal class TestHandler : IHandles<TestMessage, string[]> {
            public string Input { get; set; }

            public string[] Handle(TestMessage m)
                => new [] { Input };
        }

        internal class TestAggregator : IResultAggregator<string[]> {
            public string[] Aggregate(IEnumerable<string[]> values) => values
                .SelectMany(x => x)
                .ToArray();
        }
    }
}
