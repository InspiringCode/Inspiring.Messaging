using System;

namespace Inspiring.Messaging.Core {
    public interface IMessageProcessor<M, R> : IMessageMiddleware where M : IMessage<M, R> {
        R Process(M m, MessageContext context, Func<M, MessageContext, R> next);
    }
}