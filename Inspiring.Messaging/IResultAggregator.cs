using System;
using System.Collections.Generic;
using System.Text;

namespace Inspiring.Messaging {
    public interface IResultAggregator<R> {
        R Aggregate(IEnumerable<R> values);
    }
}
