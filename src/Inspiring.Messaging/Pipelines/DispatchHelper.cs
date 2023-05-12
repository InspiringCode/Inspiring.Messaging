namespace Inspiring.Messaging.Pipelines;

public static class PipelineExtensions {
    public static Pipeline<M, R, O, C> GetOrAdd<M, R, O, C>(
        this IPipelineCache cache,
        PipelineFactory factory
    ) where M : IMessage<M, R> {
        PipelineKey key = new(typeof(M), typeof(O), typeof(C));
        return (Pipeline<M, R, O, C>)(
            cache.Get(key) ??
            cache.Set(key, factory.Create<M, R, O, C>));
    }
}
