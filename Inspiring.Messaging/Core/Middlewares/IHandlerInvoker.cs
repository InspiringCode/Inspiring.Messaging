using System;

namespace Inspiring.Messaging.Core {
    public interface IHandlerInvoker<M, R> : IMessageMiddleware<M, R> where M : IMessage<M, R> {
        R Invoke(
            M m,
            PipelineParameters ps,
            IHandles<M, R> h, 
            Func<M, PipelineParameters, IHandles<M, R>, R> next);
    }
}