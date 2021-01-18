using System;
using System.Collections.Generic;

namespace Inspiring.Messaging.Core {
    public interface IMessageDispatcher<M, R> where M : IMessage<M, R> {
        IEnumerable<R> Dispatch(
            M m,
            PipelineParameters ps,
            IEnumerable<IHandles<M, R>> handlers,
            Func<M, PipelineParameters, IEnumerable<IHandles<M, R>>, IEnumerable<R>> next);
    }
}