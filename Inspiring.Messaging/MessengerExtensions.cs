using Inspiring.Messaging.Core;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Inspiring.Messaging {
    using static Expression;

    public static class IMessengerExtensions {
        public static R DynamicSend<R>(this IMessenger messenger, IMessage message) =>
            MessengerInvoker<R>.Send(
                messenger ?? throw new ArgumentNullException(nameof(messenger)),
                message ?? throw new ArgumentNullException(nameof(message)));

        private static class MessengerInvoker<TResult> {
            private static readonly MethodInfo __sendMethod = typeof(IMessenger)
                .GetMethod(nameof(IMessenger.Send));

            private static readonly ConcurrentDictionary<Type, Func<IMessenger, IMessage, TResult>> __cache = new();

            public static TResult Send(IMessenger messenger, IMessage message)
                => __cache.GetOrAdd(message.GetType(), CreateInvokeFunc)(messenger, message);

            private static Func<IMessenger, IMessage, TResult> CreateInvokeFunc(Type messageType) {
                ParameterExpression messenger = Parameter(typeof(IMessenger), "messenger");
                ParameterExpression message = Parameter(typeof(IMessage), "message");

                Type[] messageTypeArguments = messageType.GetInterfaces()
                    .Single(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMessage<,>))
                    .GetGenericArguments();

                MethodInfo sendMethod = __sendMethod.MakeGenericMethod(messageTypeArguments);

                return Lambda<Func<IMessenger, IMessage, TResult>>(
                    body: Call(messenger, sendMethod, Convert(message, messageType)),
                    messenger,
                    message
                ).Compile();
            }
        }
    }
}
