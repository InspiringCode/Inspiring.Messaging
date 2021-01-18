using System;
using System.Collections.Generic;
using System.Text;

namespace Inspiring.Messaging.Core {
    class MessagePipeline<M, R> where M : IMessage<M, R> {
        public Func<M, PipelineParameters, R> _processPipeline;

        public MessagePipeline(
            IEnumerable<IMessageMiddleware<M, R>> middlewares, 
            IEnumerable<IHandles<M, R>> handlers, 
            IResultAggregator<R> aggregator
        ) {

        }

        public R Process(M m, PipelineParameters ps)
            => _processPipeline(m, ps);



    }
}
