using System;
using System.Collections.Generic;

namespace Inspiring.Messaging.Core {
    public interface IHandlerProvider<in TMessageBase, in TResultBase> : IMessageMiddleware {
        IEnumerable<IHandles<M, R>> GetHandlers<M, R>(
            M m,
            MessageContext context,
            Func<M, MessageContext, IEnumerable<IHandles<M, R>>> next
        )
            where M : TMessageBase, IMessage<M, R>
            where R : TResultBase;
    }
}