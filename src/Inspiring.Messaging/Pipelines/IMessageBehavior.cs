using System;
using System.Collections.Generic;
using System.Text;

namespace Inspiring.Messaging.Pipelines {
    public interface IMessageBehavior {
        void Configure<M, R, O, C>(PipelineBuilder<M, R, O, C> pipeline);
    }
}
