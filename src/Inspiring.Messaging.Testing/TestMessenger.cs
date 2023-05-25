using Inspiring.Messaging.Behaviors.Phases;
using Inspiring.Messaging.Core;
using Inspiring.Messaging.Pipelines;
using System;
using System.ComponentModel.Design;
using System.Linq;
using System.Threading.Tasks;

namespace Inspiring.Messaging.Testing;

public class TestMessenger : IMessenger {
    private readonly IMessenger _inner;
    private readonly TestMessageHandler _handler;

    public TestMessenger(IMessenger inner) {
        const string TestBehaviorMissingText = $"The pipeline of the given 'IMessenger' does not " +
            $"contain the '{nameof(TestMessengerBehavior)}'. If you use dependency injection, add " +
            $"the following singleton registrations to your container: '{nameof(PipelineFactory)}', " +
            $"'{nameof(PipelineCache)}' (as '{nameof(IPipelineCache)}') and " +
            $"'{nameof(TestMessengerBehavior)}' (as '{nameof(IMessageBehavior)}'). If you are not " +
            $"using dependency injection, make sure that you pass a '{nameof(PipelineFactory)}' " +
            $"with a '{nameof(TestMessengerBehavior)}' to the constructor of '{nameof(Messenger)}'.";

        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        
        InitializeTestMessenger init = new();
        try {
            // If no pipeline step for the given phase is registered, an ArgumentException is thrown
            // by the 'PipelineBuilder'.
            init = _inner.Dispatcher.Dispatch<InitializeTestMessenger, object, object, InitializeTestMessenger>(
                message: init,
                operation: new object(),
                phase: init);
        } catch (ArgumentException? ex) {
            throw new ArgumentException(TestBehaviorMissingText, nameof(inner), ex);
        }
                
        _handler = 
            init.Handler ?? 
            throw new ArgumentException(TestBehaviorMissingText, nameof(inner));
    }

    public TestMessenger(params IMessageBehavior[] additionalBehaviors)
        : this(CreateMessenger(additionalBehaviors)) { }

    /// <inheritdoc />
    public IMessageDispatcher Dispatcher
        => _inner.Dispatcher;

    /// <inheritdoc />
    public ValueTask<R> PublishAsync<M, R>(IMessage<M, R> message) where M : IMessage<M, R>
        => _inner.PublishAsync(message);

    /// <inheritdoc />
    public ValueTask<R> SendAsync<M, R>(IMessage<M, R> message) where M : IMessage<M, R>
        => _inner.SendAsync(message);


    public TestMessenger Handle<M, R>(Func<M, R> handler) where M : IMessage<M, R> {
        _handler.Handlers.Add(new DelegateHandler<M, R> { Delegate = handler });
        return this;
    }

    public TestMessenger Handle<M, R>(Func<M, Task<R>> handler) where M : IMessage<M, R> {
        _handler.Handlers.Add(new DelegateHandlerAsync<M, R> { Delegate = handler });
        return this;
    }

    public TestMessenger Handle<M>(Action<M> handler) where M : IMessage {
        _handler.Spies.Add(handler);
        return this;
    }

    public TestMessenger Handle<M>(Func<M, Task> handler) where M : IMessage {
        _handler.Spies.Add(handler);
        return this;
    }

    public TestMessenger Filter<M, R>(Func<M, MessengerArgs, Func<R?>, R?> filter) where M : IMessage<M, R> {
        _handler.Filters.Add(filter);
        return this;
    }

    public TestMessenger FilterAsync<M, R>(Func<M, MessengerArgs, Func<Task<R>>, Task<R>> filter) where M : IMessage<M, R> {
        _handler.Filters.Add(filter);
        return this;
    }


    private static IMessenger CreateMessenger(IMessageBehavior[] additionalBehaviors) {
        IMessageBehavior[] behaviors = new[] { new TestMessengerBehavior() }
            .Concat(additionalBehaviors)
            .ToArray();

        PipelineFactory f = new(behaviors);
        PipelineCache c = new();
        return new Messenger(new NullServiceProvider(), f, c);
    }

    internal class InitializeTestMessenger : IMessage<InitializeTestMessenger, object> { 
        public TestMessageHandler? Handler { get; set; }
    }

    private class DelegateHandler<M, R> : IHandles<M, R> where M : IMessage<M, R> {
        public required Func<M, R> Delegate { get; init; }

        public R Handle(M message)
            => Delegate(message);
    }

    private class DelegateHandlerAsync<M, R> : IHandlesAsync<M, R> where M : IMessage<M, R> {
        public required Func<M, Task<R>> Delegate { get; init; }

        public Task<R> Handle(M message)
            => Delegate(message);
    }

    private class NullServiceProvider : IServiceProvider {
        public static readonly NullServiceProvider Instance = new();

        public object? GetService(Type serviceType) => null;
    }
}

public record MessengerArgs(object Operation, object Context) { }
