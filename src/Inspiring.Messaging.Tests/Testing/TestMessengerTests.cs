using Inspiring.Messaging.Behaviors.Phases;
using Inspiring.Messaging.Core;
using Inspiring.Messaging.Pipelines;
using Inspiring.Messaging.Synchronous;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Inspiring.Messaging.Testing;

public class TestMessengerTests : FeatureBase {
    [Scenario]
    internal void TestMessengerWithActualMessenger(
        Messenger m,
        TestMessenger tm,
        TestServiceProvider sp,
        Action act,
        List<string> invokeHandlerResults,
        bool actionWasCalled,
        bool asyncActionWasCalled,
        bool handlerWasCalled,
        string result
    ) {
        GIVEN["an actual messenger"] = () => m = new Messenger(
            sp = new(),
            new PipelineFactory(new IMessageBehavior[] {
                new TestMessengerBehavior(),
                new DelegateBehavior<TestMessage, string, PublishOperation, DefaultContext, InvokeHandler<string>>(
                    (i, p) => (invokeHandlerResults ??= new()).Add(p.Result!)
                )
            }),
            new PipelineCache());
        AND["a test messenger"] = () => tm = new TestMessenger(m);

        WHEN["the inner messenger does not have the test behavior"] = () => act = () => new TestMessenger(new Messenger(sp));
        THEN["an exception is thrown"] = () => act
            .Should().Throw<ArgumentException>().WithMessage($"*{nameof(TestMessengerBehavior)}*");

        GIVEN["a sync and async handlers that do not return a result"] = () => tm
            .Handle<TestMessage>(m => actionWasCalled = true)
            .Handle<TestMessage>(m => {
                asyncActionWasCalled = true;
                return Task.CompletedTask;
            });

        WHEN["publishing the message"] = () => m.PublishAsync(new TestMessage());
        THEN["the sync test handler is called"] = () => actionWasCalled.Should().BeTrue();
        AND["the async test handler is called"] = () => asyncActionWasCalled.Should().BeTrue();

        WHEN["no normal handlers are registered and sending the message"] = () => act = () => m.Send(new TestMessage());
        THEN["an exception is thrown"] = () => act.Should().ThrowExactly<MessengerException>().WithMessage("*handler*");

        GIVEN["a sync and async handler that return a result"] = () => tm
            .Handle<TestMessage, string>(m => {
                handlerWasCalled = true;
                return "Sync";
            })
            .Handle<TestMessage, string>(m => {
                handlerWasCalled = true;
                return Task.FromResult("Async");
            });

        WHEN["publishing the message"] = () => m.PublishAsync(new TestMessage());
        THEN["the test handlers are actual handlers for which all pipeline steps are run"] = () =>
            invokeHandlerResults.Should().BeEquivalentTo("Sync", "Async");

        GIVEN["a filter that calls next"] = () => {
            actionWasCalled = asyncActionWasCalled = handlerWasCalled = false;
            tm.Filter<TestMessage, string>((m, args, next) => next());
            tm.FilterAsync<TestMessage, string>((m, args, next) => next());
        };
        WHEN["sending a message sync"] = () => result = m.Send(new TestMessage());
        THEN["all handlers are still invoked"] = () => {
            actionWasCalled.Should().BeTrue();
            handlerWasCalled.Should().BeTrue();
            result.Should().Be("Sync");
        };

        WHEN["sending a message async"] = async () => result = await m.SendAsync(new TestMessage());
        THEN["all handlers are still invoked"] = () => {
            asyncActionWasCalled.Should().BeTrue();
            handlerWasCalled.Should().BeTrue();
            result.Should().Be("Sync");
        };

        GIVEN["a filter that does not call next"] = () => {
            actionWasCalled = asyncActionWasCalled = handlerWasCalled = false;
            tm.Filter<TestMessage, string>((m, args, next) => "Filtered-Sync");
            tm.FilterAsync<TestMessage, string>((m, args, next) => Task.FromResult("Filtered-Async"));
        };

        WHEN["sending a message sync"] = () => result = m.Send(new TestMessage());
        THEN["the filtered result is returned"] = () => result.Should().Be("Filtered-Sync");
        AND["no handlers are invoked"] = () => {
            actionWasCalled.Should().BeFalse();
            handlerWasCalled.Should().BeFalse();
        };

        WHEN["sending a message async"] = async () => result = await m.SendAsync(new TestMessage());
        THEN["the filtered result is returned"] = () => result.Should().Be("Filtered-Async");
        AND["no handlers are invoked"] = () => {
            asyncActionWasCalled.Should().BeFalse();
            handlerWasCalled.Should().BeFalse();
        };
    }

    [Scenario]
    internal void StandaloneTestMessenger(TestMessenger m, string result) {
        GIVEN["a standalone test messenger"] = () => m = new TestMessenger();
        AND["a test handler"] = () => m.Handle<TestMessage, string>(m => "test-result");

        WHEN["sending a message"] = async () => result = await m.SendAsync(new TestMessage());
        THEN["the test handler is invoked"] = () => result.Should().Be("test-result");
    }


    internal class TestMessage : IMessage<TestMessage, string> { }
}
