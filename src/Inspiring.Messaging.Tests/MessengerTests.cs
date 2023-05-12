using Inspiring.Messaging.Behaviors.Phases;
using Inspiring.Messaging.Core;
using Inspiring.Messaging.Pipelines;
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
    internal void SendMessage(IHandles<TestMessage, string> h, string result, TestMessage m) {
        GIVEN["a handler"] = () => {
            _services.Add(h = Substitute.For<IHandles<TestMessage, string>>());
            h.Handle(default).ReturnsForAnyArgs("test-result");
        };
        WHEN["sending a message"] = () => result = _messenger.SendSync(m = new TestMessage());
        THEN["the handler is invoked exactly once"] = () => h.Received(1).Handle(m);
        AND["the result of the handler is returned"] = () => result.Should().Be("test-result");

        //WHEN["no handlers are registered for a message"] = () => _services.Clear();
        //THEN["sending a message throws an exception"] = () => new Action(() => _messenger.SendSync(new TestMessage()))
        //    .Should().Throw<MessengerException>();
    }

    [Scenario]
    internal void SendMessageAsync(IHandlesAsync<TestMessage, string> h, string result, TestMessage m) {
        GIVEN["a handler"] = () => {
            _services.Add(h = Substitute.For<IHandlesAsync<TestMessage, string>>());
            h.Handle(default).ReturnsForAnyArgs("test-result");
        };
        WHEN["sending a message"] = async () => result = await _messenger.SendAsync(m = new TestMessage());
        THEN["the handler is invoked exactly once"] = () => h.Received(1).Handle(m);
        AND["the result of the handler is returned"] = () => result.Should().Be("test-result");
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
