using System;

namespace Inspiring.Messaging.Core {
    public interface IMessageProcessor : IMessageMiddleware {
        R Process<M, R>(M m, MessageContext context, Func<M, MessageContext, R> next) where M : IMessage<M, R>;
    }
}