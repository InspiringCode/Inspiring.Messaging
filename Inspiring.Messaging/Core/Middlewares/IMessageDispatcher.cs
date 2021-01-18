using System.Collections.Generic;

namespace Inspiring.Messaging.Core {
    public interface IMessageDispatcher<M, R> where M : IMessage<M, R> {
        IEnumerable<R> Process(M m, IEnumerable<IHandles<M, R>> handlers, PipelineParameters ps);
    }
}