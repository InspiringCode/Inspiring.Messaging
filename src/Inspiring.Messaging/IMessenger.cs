using Inspiring.Messaging.Core;
using System.Threading.Tasks;

namespace Inspiring.Messaging {
    public interface IMessenger {
        IMessageDispatcher Dispatcher { get; }

        ValueTask<R> SendAsync<M, R>(IMessage<M, R> message) where M : IMessage<M, R>;
    }
}
