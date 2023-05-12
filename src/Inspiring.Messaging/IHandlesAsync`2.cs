using Inspiring.Messaging.Core;
using System.Threading.Tasks;

namespace Inspiring.Messaging;

public interface IHandlesAsync<in M, R> : IHandles<M> where M : IMessage<M, R> {
    Task<R> Handle(M message);
}
