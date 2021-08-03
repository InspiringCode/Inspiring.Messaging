using FluentAssertions;
using NSubstitute;
using System;
using Xbehave;

namespace Inspiring.Messaging.Tests.Messenger {
    public class MessengerTests : Feature {
        [Scenario]
        internal void DynamicSendTests(IMessenger messenger, TestMessage msg, string expected, IComparable comparableResult, string result) {
            GIVEN["a mock messenger"] |= () => {
                messenger = Substitute.For<IMessenger>();
                messenger.Send(msg = new TestMessage()).Returns(expected = "TEST");
            };

            WHEN["calling DynamicSend"] |= () => result = messenger.DynamicSend<string>(msg);
            THEN["the strongly typed Send is called"] |= () => result.Should().Be(expected);

            WHEN["calling DynamicSend with a result base class or interface"] |= () => comparableResult = messenger.DynamicSend<IComparable>(msg);
            THEN["the result is casted to the specified type/interface"] |= () => result.Should().Be(expected);
        }

        internal class TestMessage : IMessage<TestMessage, string> {

        }
    }
}
