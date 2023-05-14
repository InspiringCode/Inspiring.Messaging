using Inspiring.Messaging.Core;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Inspiring.Messaging.Dynamic;

public class DynamicSendTests : FeatureBase {
    [Scenario]
    internal void DynamicSend(IMessenger messenger, IHandles<TestMessage, int> h, TestMessage m, int result) {
        USING["a messenger"] = () => messenger = 
            new Messenger(
                new TestServiceProvider {  
                    (h = Substitute.For<IHandles<TestMessage, int>>())
                });
        GIVEN["a handler"] = () => h.Handle(m = new TestMessage()).Returns(1234);
        WHEN["sending a weakly typed message"] = () => result = messenger.SendDynamic<int>((IMessage)m);
        THEN["the result is returned"] = () => result.Should().Be(1234);
    }

    internal class TestMessage : IMessage<TestMessage, int> { }
}
