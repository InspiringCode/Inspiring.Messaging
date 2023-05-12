using System;
using System.Collections.Generic;
using System.Text;

namespace Inspiring.Messaging.Pipelines {
    public interface IStepInfo {
        int Order { get; }

        object? Tag { get; }
    }
}
