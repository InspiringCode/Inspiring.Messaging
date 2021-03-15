using System;

namespace Inspiring.Messaging.Core {
    public interface IHandlerInvoker<M, R> : IMessageMiddleware where M : IMessage<M, R> {
        R Invoke(
            M m,
            MessageContext context,
            IHandles<M, R> h, 
            Func<M, MessageContext, IHandles<M, R>, R> next);
    }
}