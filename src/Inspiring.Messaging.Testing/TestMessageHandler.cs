using Inspiring.Messaging.Behaviors.Phases;
using Inspiring.Messaging.Core;
using Inspiring.Messaging.Pipelines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using static Inspiring.Messaging.Testing.TestMessenger;

namespace Inspiring.Messaging.Testing;

internal class TestMessageHandler {
    public List<IHandler> Handlers { get; } = new();
    
    public List<Delegate> Spies { get; } = new();

    public List<Delegate> Filters { get; } = new();
    
    public static void AddToPipeline<M, R, O, C>(PipelineBuilder<M, R, O, C> pipeline, TestMessageHandler handler) {
        if (pipeline is PipelineBuilder<InitializeTestMessenger, object, object, C> p)
            p.AddStep<InitializeTestMessenger>(next => new InitializationStep<C>(handler));

        pipeline.AddStep<ProcessMessage<R>>(next => new ProcessStep<M, R, O, C>(next, handler), order: -100_000);
        pipeline.AddStep<ProvideHandlers>(next => new HandlerProviderStep<M, R, O, C>(next, handler), order: -100_000);
        pipeline.AddStep<InvokeHandlers<R>>(next => new HandlersInvokerStep<M, R, O, C>(next, handler), order: -100_000);
    }


    private class ProcessStep<M, R, O, C> : PipelineStep<M, R, O, C, ProcessMessage<R>> {
        private readonly PipelineStep<M, R, O, C, ProcessMessage<R>> _next;
        private readonly TestMessageHandler _handler;

        internal ProcessStep(PipelineStep<M, R, O, C, ProcessMessage<R>> next, TestMessageHandler handler)
            => (_next, _handler) = (next, handler);

        public override ProcessMessage<R> Invoke(Invocation<M, R, O, C> i, ProcessMessage<R> phase) {
            MessengerArgs args = new(i.Operation!, i.Context!);
            
            var filters = _handler
                .Filters
                .OfType<Func<M, MessengerArgs, Func<R>, R>>();

            Func<R> invoke = filters.Aggregate(
                seed: () => _next.Invoke(i, phase).Result,
                (next, filter) => {
                    return () => filter(i.Message, args, next);
                });

            return new ProcessMessage<R>(invoke());
        }

        public override async ValueTask<ProcessMessage<R>> InvokeAsync(Invocation<M, R, O, C> i, ProcessMessage<R> phase) {
            MessengerArgs args = new(i.Operation!, i.Context!);

            var filters = _handler
                .Filters
                .OfType<Func<M, MessengerArgs, Func<Task<R>>, Task<R>>>();

            Func<Task<R>> invoke = filters.Aggregate(
                seed: async () => (await _next.InvokeAsync(i, phase)).Result,
                (next, filter) => {
                    return () => filter(i.Message, args, next);
                });

            return new ProcessMessage<R>(await invoke());
        }
    }

    private class HandlerProviderStep<M, R, O, C> : PipelineStep<M, R, O, C, ProvideHandlers> {
        private readonly PipelineStep<M, R, O, C, ProvideHandlers> _next;
        private readonly TestMessageHandler _handler;

        public HandlerProviderStep(PipelineStep<M, R, O, C, ProvideHandlers> next, TestMessageHandler handler)
            => (_next, _handler) = (next, handler);

        public override ProvideHandlers Invoke(Invocation<M, R, O, C> i, ProvideHandlers phase)
            => AddHandlers(_next.Invoke(i, phase));

        public override async ValueTask<ProvideHandlers> InvokeAsync(Invocation<M, R, O, C> i, ProvideHandlers phase)
            => AddHandlers(await _next.InvokeAsync(i, phase));

        private ProvideHandlers AddHandlers(ProvideHandlers phase) {
            IHandler[] handlers = _handler
                .Handlers.OfType<IHandles<M>>()
                .Concat(phase.Handlers)
                .ToArray();

            return phase with { Handlers = handlers };
        }
    }

    private class HandlersInvokerStep<M, R, O, C> : PipelineStep<M, R, O, C, InvokeHandlers<R>> {
        private readonly PipelineStep<M, R, O, C, InvokeHandlers<R>> _next;
        private readonly TestMessageHandler _handler;

        public HandlersInvokerStep(PipelineStep<M, R, O, C, InvokeHandlers<R>> next, TestMessageHandler handler)
            => (_next, _handler) = (next, handler);

        public override InvokeHandlers<R> Invoke(Invocation<M, R, O, C> i, InvokeHandlers<R> phase) {
            foreach (Action<M> action in _handler.Spies.OfType<Action<M>>())
                action(i.Message);

            return _next.Invoke(i, phase);
        }

        public override async ValueTask<InvokeHandlers<R>> InvokeAsync(Invocation<M, R, O, C> i, InvokeHandlers<R> phase) {
            foreach (Action<M> action in _handler.Spies.OfType<Action<M>>())
                action(i.Message);

            foreach (Func<M, Task> action in _handler.Spies.OfType<Func<M, Task>>())
                await action(i.Message);


            return await _next.InvokeAsync(i, phase);
        }
    }

    private class InitializationStep<TContext> : PipelineStep<
        InitializeTestMessenger,
        object,
        object,
        TContext,
        InitializeTestMessenger> {

        private readonly TestMessageHandler _handler;

        public InitializationStep(TestMessageHandler handler) => _handler = handler;

        public override InitializeTestMessenger Invoke(
            Invocation<InitializeTestMessenger, object, object, TContext> i,
            InitializeTestMessenger phase
        ) {
            phase.Handler = _handler;
            return phase;
        }

        public override ValueTask<InitializeTestMessenger> InvokeAsync(
            Invocation<InitializeTestMessenger, object, object, TContext> i,
            InitializeTestMessenger phase
        ) => throw new NotSupportedException();
    }
}