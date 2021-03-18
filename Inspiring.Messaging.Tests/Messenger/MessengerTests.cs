using FluentAssertions;
using NSubstitute;
using Xbehave;

namespace Inspiring.Messaging.Tests.Messenger {
    public class MessengerTests : Feature {
        [Scenario]
        internal void DynamicSendTests(IMessenger messenger, TestMessage msg, string expected, string result) {
            GIVEN["a mock messenger"] |= () => {
                messenger = Substitute.For<IMessenger>();
                messenger.Send(msg = new TestMessage()).Returns(expected = "TEST");
            };

            WHEN["calling DynamicSend"] |= () => result = messenger.DynamicSend<string>(msg);
            THEN["the strongly typed Send is called"] |= () => result.Should().Be(expected);
        }

        internal class TestMessage : IMessage<TestMessage, string> {

        }
    }
}
