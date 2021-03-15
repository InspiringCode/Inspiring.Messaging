using System;
using System.Collections.Generic;

namespace Inspiring.Messaging.Core {
    public interface IMessageDispatcher<M, R> : IMessageMiddleware where M : IMessage<M, R> {
        IEnumerable<R> Dispatch(
            M m,
            MessageContext context,
            IEnumerable<IHandles<M, R>> handlers,
            Func<M, MessageContext, IEnumerable<IHandles<M, R>>, IEnumerable<R>> next);
    }
}