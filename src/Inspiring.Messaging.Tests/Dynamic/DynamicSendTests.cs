using Inspiring.Messaging.Testing;

namespace Inspiring.Messaging.Dynamic;

public class DynamicSendTests : FeatureBase {
    [Scenario]
    internal void DynamicSend(
        TestMessenger m, 
        TestMessage msg, 
        int result,
        object objectResult
    ) {
        USING["a messenger with a handler"] = () => m = new TestMessenger()
            .Handle<TestMessage, int>(m => 1337);

        WHEN["sending a weakly typed message"] = () => result = m.SendDynamic<int>(msg = new());
        THEN["the result is returned"] = () => result.Should().Be(1337);

        WHEN["sending it async"] = async () => result = await m.SendDynamicAsync<int>(msg);
        THEN["the result is returned"] = () => result.Should().Be(1337);

        WHEN["publishing it"] = () => result = m.PublishDynamic<int>(msg);
        THEN["the result is returned"] = () => result.Should().Be(1337);

        WHEN["publishing it async"] = async () => result = await m.PublishDynamicAsync<int>(msg);
        THEN["the result is returned"] = () => result.Should().Be(1337);

        WHEN["specifying a base type as result type"] = () => objectResult = m.SendDynamic<object>(msg);
        THEN["the result is returned"] = () => objectResult.Should().Be(1337);
    }

    internal class TestMessage : IMessage<TestMessage, int> { }
}
