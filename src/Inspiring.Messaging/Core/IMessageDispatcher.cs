using System.Threading.Tasks;

namespace Inspiring.Messaging.Core;

public interface IMessageDispatcher {
    P Dispatch<M, R, O, P>(M message, in O operation, in P phase) where M : IMessage<M, R>;

    ValueTask<P> DispatchAsync<M, R, O, P>(M message, in O operation, in P phase) where M : IMessage<M, R>;
}
