using System.Collections.Generic;

namespace Inspiring.Messaging.Core {
    public interface IHandlerProvider<M, R> where M : IMessage<M, R> {
        IEnumerable<IHandles<M, R>> GetHandlers(M m, PipelineParameters ps);
    }
}