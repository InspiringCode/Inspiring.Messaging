using System;
using System.Collections.Generic;

namespace Inspiring.Messaging.Core {
    public interface IMessageResultAggregator<R> : IMessageMiddleware {
        R Aggregate<M>(
            M m,
            MessageContext context,
            IEnumerable<R> results,
            Func<M, MessageContext, IEnumerable<R>, R> next
        ) where M : IMessage<M, R>;
    }
}