﻿using System;
using System.Collections.Generic;

namespace Inspiring.Messaging.Core {
    public interface IMessageDispatcher : IMessageMiddleware {
        IEnumerable<R> Dispatch<M, R>(
            M m,
            MessageContext context,
            IEnumerable<IHandles<M, R>> handlers,
            Func<M, MessageContext, IEnumerable<IHandles<M, R>>, IEnumerable<R>> next
        ) where M : IMessage<M, R>;
    }
}