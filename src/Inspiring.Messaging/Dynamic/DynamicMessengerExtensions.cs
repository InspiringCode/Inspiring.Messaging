using Inspiring.Messaging.Core;
using Inspiring.Messaging.Synchronous;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace Inspiring.Messaging.Dynamic;

public static class DynamicMessengerExtensions {
    public static TResult SendDynamic<TResult>(this IMessenger messenger, IMessage message)
        => DynamicMessageFunction.Invoke(message, DynamicFunction<TResult>.Send, messenger);

    public static ValueTask<TResult> SendDynamicAsync<TResult>(this IMessenger messenger, IMessage message)
        => DynamicMessageFunction.InvokeAsync(message, DynamicFunction<TResult>.Send, messenger);


    public static TResult PublishDynamic<TResult>(this IMessenger messenger, IMessage message)
        => DynamicMessageFunction.Invoke(message, DynamicFunction<TResult>.Publish, messenger);


    public static ValueTask<TResult> PublishDynamicAsync<TResult>(this IMessenger messenger, IMessage message)
        => DynamicMessageFunction.InvokeAsync(message, DynamicFunction<TResult>.Publish, messenger);


    private class DynamicFunction<TResult> : IMessageFunction<IMessenger, TResult> {
        public static readonly DynamicFunction<TResult> Send = new() { _publish = false };
        public static readonly DynamicFunction<TResult> Publish = new() { _publish = true };

        private bool _publish;

        public TResult Invoke<M, R>(M message, IMessenger messenger) where M : IMessage<M, R> {
            R result = _publish ?
                messenger.Publish(message) :
                messenger.Send(message);

            return result switch {
                TResult r => r,
                null => TryCastNull(message),
                _ => ThrowCannotCastMessageResult(message, result.GetType())
            };
        }

        public async ValueTask<TResult> InvokeAsync<M, R>(M message, IMessenger messenger) where M : IMessage<M, R> {
            R result = _publish ?
                await messenger.PublishAsync(message) :
                await messenger.SendAsync(message);

            return result switch {
                TResult r => r,
                null => TryCastNull(message),
                _ => ThrowCannotCastMessageResult(message, result.GetType())
            };
        }

        [DebuggerStepThrough]
        private static TResult TryCastNull(IMessage message) {
            if (typeof(TResult).IsValueType) {
                throw new InvalidCastException($"The dynamic send or publish of message '{message.GetType().Name}' " +
                    $"was expected to return a result of type '{typeof(TResult).Name}' but it returned null.");
            }

            return default!;
        }

        [DebuggerStepThrough]
        private static TResult ThrowCannotCastMessageResult(IMessage message, Type actualResultType) {
            throw new InvalidCastException($"The dynamic send or publish of message '{message.GetType().Name}' " +
                $"was expected to return a result of type '{typeof(TResult).Name}' but it returned a value of " +
                $"type '{actualResultType.Name}'.");
        }
    }
}
