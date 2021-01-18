using System.Collections.Generic;

namespace Inspiring.Messaging.Core {
    public interface IMessageResultAggregator<M, R> where M : IMessage<M, R> {
        R Aggregate(IEnumerable<R> results,  M m, PipelineParameters ps);
    }
}