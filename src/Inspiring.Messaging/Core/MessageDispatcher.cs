using Inspiring.Messaging.Pipelines;
using System;
using System.Threading.Tasks;

namespace Inspiring.Messaging.Core;

public class MessageDispatcher : IMessageDispatcher {
    private readonly IServiceProvider _services;
    private readonly PipelineFactory _factory;
    private readonly IPipelineCache _pipelines;

    public MessageDispatcher(IServiceProvider services)
        : this(services, PipelineFactory.DefaultFactory, PipelineCache.DefaultCache) { }

    public MessageDispatcher(IServiceProvider services, PipelineFactory factory, IPipelineCache pipelines) {
        _services = services ?? throw new ArgumentNullException(nameof(services));
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        _pipelines = pipelines ?? throw new ArgumentNullException(nameof(pipelines));
    }

    public P Dispatch<M, R, O, P>(M message, in O operation, in P phase) where M : IMessage<M, R> {
        PipelineKey key = new(typeof(M), typeof(O), typeof(DefaultContext));

        Pipeline<M, R, O, DefaultContext> p = (Pipeline<M, R, O, DefaultContext>)(
            _pipelines.Get(key) ??
            _pipelines.Set(key, _factory.Create<M, R, O, DefaultContext>));

        return p.Invoke(
            new Invocation<M, R, O, DefaultContext>(
                message,
                operation,
                new DefaultContext(_services),
                p),
            phase);
    }

    public ValueTask<P> DispatchAsync<M, R, O, P>(M message, in O operation, in P phase) where M : IMessage<M, R> {
        PipelineKey key = new(typeof(M), typeof(O), typeof(DefaultContext));

        Pipeline<M, R, O, DefaultContext> p = (Pipeline<M, R, O, DefaultContext>)(
            _pipelines.Get(key) ??
            _pipelines.Set(key, _factory.Create<M, R, O, DefaultContext>));

        return p.InvokeAsync(
            new Invocation<M, R, O, DefaultContext>(
                message,
                operation,
                new DefaultContext(_services),
                p),
            phase);
    }
}
