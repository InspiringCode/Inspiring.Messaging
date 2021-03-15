using System;
using System.Collections.Generic;

namespace Inspiring.Messaging.Core {
    public interface IHandlerProvider<M, R> : IMessageMiddleware where M : IMessage<M, R> {
        IEnumerable<IHandles<M, R>> GetHandlers(
            M m, 
            MessageContext context, 
            Func<M, MessageContext, IEnumerable<IHandles<M, R>>> next);
    }
}