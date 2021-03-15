using System.Collections.Generic;
using System.Linq;

namespace Inspiring.Messaging.Core {
    public class TakeLastResultAggregator<R> : IResultAggregator<R> {
        public static readonly TakeLastResultAggregator<R> Instance = new();

        public R Aggregate(IEnumerable<R> values)
            => values.Last();
    }
}
