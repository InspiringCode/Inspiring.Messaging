using System;

namespace Inspiring.Messaging.Core {
    public interface IMessageProcessor<in TMessageBase, in TResultBase> : IMessageMiddleware {
        R Process<M, R>(M m, MessageContext context, Func<M, MessageContext, R> next) 
            where M : TMessageBase, IMessage<M, R> 
            where R : TResultBase;
    }
}