namespace Inspiring.Messaging.Core {
    public interface IHandlerInvoker<M, R> where M : IMessage<M, R> {
        R Process(M m, PipelineParameters ps);
    }
}