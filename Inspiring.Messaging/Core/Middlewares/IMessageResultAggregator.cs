using System;
using System.Collections.Generic;

namespace Inspiring.Messaging.Core {
    public interface IMessageResultAggregator<M, R> : IMessageMiddleware<M, R> where M : IMessage<M, R> {
        R Aggregate(
            M m,
            PipelineParameters ps, 
            IEnumerable<R> results,
            Func<M, PipelineParameters, IEnumerable<R>, R> next
        );
    }
}