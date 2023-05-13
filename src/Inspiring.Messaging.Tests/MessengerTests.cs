using Inspiring.Messaging.Behaviors.Phases;
using Inspiring.Messaging.Core;
using Inspiring.Messaging.Synchronous;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NSubstitute.Routing.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inspiring.Messaging;

public class MessengerTests : FeatureBase {
    private Messenger _messenger;
    private TestServiceProvider _services;

    [Background]
    public void Background() {
        USING["a service provider"] = () => _services = new TestServiceProvider();
        AND["a messenger"] = () => _messenger = new Messenger(_services);
    }

    [Scenario]
    internal void SendMessage(
        IHandles<TestMessage, string> h1,
        IHandles<TestMessage, string> h2,
        string result, 
        TestMessage m
    ) {
        GIVEN["a handler"] = () => {
            _services.Add(h1 = Substitute.For<IHandles<TestMessage, string>>());
            _services.Add(h2 = Substitute.For<IHandles<TestMessage, string>>());
            h1.Handle(default).ReturnsForAnyArgs("test-result-1");
            h2.Handle(default).ReturnsForAnyArgs("test-result-2");
        };
        WHEN["sending a message"] = () => result = _messenger.Send(m = new TestMessage());
        THEN["all handlers are invoked exactly once"] = () => {
            h1.Received(1).Handle(m);
            h2.Received(1).Handle(m);
        };
        AND["the result of the first handler is returned"] = () => result.Should().Be("test-result-1");

        WHEN["publishing a message"] = () => {
            h1.ClearReceivedCalls();
            h2.ClearReceivedCalls();
            result = _messenger.Publish(m);
        };
        THEN["all handlers are invoked exactly once"] = () => {
            h1.Received(1).Handle(m);
            h2.Received(1).Handle(m);
        };
        AND["the result of the first handler is returned"] = () => result.Should().Be("test-result-1");

        WHEN["no handlers are registered for a message"] = () => _services.Clear();
        THEN["sending a message throws an exception"] = () => new Action(() => _messenger.Send(new TestMessage()))
            .Should().ThrowExactly<MessengerException>();
        AND["publishing a message does not throw an exception"] = () => _messenger.Publish(m);
    }

    [Scenario]
    internal void SendMessageAsync(
        IHandlesAsync<TestMessage, string> h1,
        IHandlesAsync<TestMessage, string> h2, 
        string result, 
        TestMessage m
    ) {
        GIVEN["a handler"] = () => {
            _services.Add(h1 = Substitute.For<IHandlesAsync<TestMessage, string>>());
            _services.Add(h2 = Substitute.For<IHandlesAsync<TestMessage, string>>());
            h1.Handle(default).ReturnsForAnyArgs("test-result-1");
            h2.Handle(default).ReturnsForAnyArgs("test-result-2");
        };

        WHEN["sending a message"] = async () => result = await _messenger.SendAsync(m = new TestMessage());
        THEN["all handlers are invoked exactly once"] = () => {
            h1.Received(1).Handle(m);
            h2.Received(1).Handle(m);
        };
        AND["the result of the first handler is returned"] = () => result.Should().Be("test-result-1");

        WHEN["publishing a message"] = async () => {
            h1.ClearReceivedCalls();
            h2.ClearReceivedCalls();
            result = await _messenger.PublishAsync(m);
        };
        THEN["all handlers are invoked exactly once"] = () => {
            h1.Received(1).Handle(m);
            h2.Received(1).Handle(m);
        };
        AND["the result of the first handler is returned"] = () => result.Should().Be("test-result-1");

        WHEN["no handlers are registered for a message"] = () => _services.Clear();
        THEN["sending a message throws an exception"] = () => new Func<Task>(async () => await _messenger.SendAsync(new TestMessage()))
            .Should().ThrowAsync<MessengerException>();
        AND["publishing a message does not throw an exception"] = () => _messenger.PublishAsync(m);
    }

    [Scenario]
    internal void NonConcurrentHandlerInvocation(
        IHandlesAsync<TestMessage, string> h1,
        IHandlesAsync<TestMessage, string> h2,
        TaskCompletionSource<string> h1Completion,
        bool h2wasChalled,
        Task<string[]> results
    ) {
        GIVEN["some handlers"] = () => {
            _services.Add(h1 = Substitute.For<IHandlesAsync<TestMessage, string>>());
            _services.Add(h2 = Substitute.For<IHandlesAsync<TestMessage, string>>());
            h1.Handle(default).ReturnsForAnyArgs(ci => { return (h1Completion = new TaskCompletionSource<string>()).Task; });
            h2.Handle(default).ReturnsForAnyArgs(ci => { h2wasChalled = true; return "R2"; });
        };

        WHEN["invoking these handlers"] = () => { results = invokeHandlers(new TestMessage(), h1, h2); };
        THEN["they are invoked non-concurrently"] = async () => {
            h2wasChalled.Should().BeFalse(because: "h1 has not completed yet");
            results.IsCompleted.Should().BeFalse();
            h1Completion.SetResult("R1");
            await results;
            h2wasChalled.Should().BeTrue();
        };
        AND["the results are returned"] = () => results.Result.Should().BeEquivalentTo("R1", "R2");


        async Task<R[]> invokeHandlers<M, R>(IMessage<M, R> m, params IHandles<M>[] handlers) where M : IMessage<M, R> => (await _messenger
            .Dispatcher.DispatchAsync<M, R, SendOperation, InvokeHandlers<R>>((M)m, new SendOperation(), new InvokeHandlers<R>(handlers)))
            .Results;
    }

    internal record TestMessage : IMessage<TestMessage, string>;
}
