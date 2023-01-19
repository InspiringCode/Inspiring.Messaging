using System;

namespace Inspiring.Messaging.Core {
    public interface IHandlerInvoker : IMessageMiddleware {
        R Invoke<M, R>(
            M m,
            MessageContext context,
            IHandles<M, R> h, 
            Func<M, MessageContext, IHandles<M, R>, R> next
        ) where M : IMessage<M, R>;
    }
}