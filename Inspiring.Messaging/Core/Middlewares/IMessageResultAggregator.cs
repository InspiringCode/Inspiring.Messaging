using System;
using System.Collections.Generic;

namespace Inspiring.Messaging.Core {
    public interface IMessageResultAggregator<in TMessageBase, R> : IMessageMiddleware {
        R Aggregate<M>(
            M m,
            MessageContext context,
            IEnumerable<R> results,
            Func<M, MessageContext, IEnumerable<R>, R> next
        ) where M : TMessageBase, IMessage<M, R>;
    }
}