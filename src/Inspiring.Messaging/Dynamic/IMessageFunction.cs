using System.Threading.Tasks;

namespace Inspiring.Messaging.Dynamic;

public interface IMessageFunction<TArg, TResult> {
    TResult Invoke<M, R>(M message, TArg arg) where M : IMessage<M, R>;

    ValueTask<TResult> InvokeAsync<M, R>(M message, TArg arg) where M : IMessage<M, R>;
}
