using System;
using System.Collections.Generic;

namespace Inspiring.Messaging.Core {
    public interface IMessageResultAggregator<M, R> : IMessageMiddleware where M : IMessage<M, R> {
        R Aggregate(
            M m,
            MessageContext context, 
            IEnumerable<R> results,
            Func<M, MessageContext, IEnumerable<R>, R> next
        );
    }
}