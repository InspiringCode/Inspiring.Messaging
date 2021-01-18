using System.Collections.Generic;

namespace Inspiring.Messaging.Core {
    public interface IResultAggregator<R> {
        R Aggregate(IEnumerable<R> values);
    }
}
