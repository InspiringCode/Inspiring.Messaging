using Inspiring.Messaging.Core;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Inspiring.Messaging.Dynamic;
public static class DynamicMessageFunction {
    private static readonly ConcurrentDictionary<Type, IDynamicFunctionInvoker> _invokers = new();

    public static TResult Invoke<TArg, TResult>(
        IMessage message,
        IMessageFunction<TArg, TResult> function,
        TArg arg
    ) {
        return _invokers
            .GetOrAdd(message.GetType(), CreateInvoker)
            .Invoke(message, function, arg);
    }

    public static ValueTask<TResult> InvokeAsync<TArg, TResult>(
        IMessage message,
        IMessageFunction<TArg, TResult> function,
        TArg arg
    ) {
        return _invokers
            .GetOrAdd(message.GetType(), CreateInvoker)
            .InvokeAsync(message, function, arg);
    }

    private static IDynamicFunctionInvoker CreateInvoker(Type messageType) {
        Type? messageInterface = messageType.GetInterface("IMessage`2");
        
        if (messageType == null) 
            throw new ArgumentException("Message type must implement 'IMessage<M, R>'.");
        
        Type invokerType = typeof(DynamicFunctionInvoker<,>)
            .MakeGenericType(messageInterface.GenericTypeArguments);

        return (IDynamicFunctionInvoker)Activator.CreateInstance(invokerType);
    }

    private interface IDynamicFunctionInvoker {
        TResult Invoke<TArg, TResult>(IMessage message, IMessageFunction<TArg, TResult> function, TArg arg);


        ValueTask<TResult> InvokeAsync<TArg, TResult>(IMessage message, IMessageFunction<TArg, TResult> function, TArg arg);
    }

    private class DynamicFunctionInvoker<M, R> : IDynamicFunctionInvoker where M : IMessage<M, R> {
        public TResult Invoke<TArg, TResult>(IMessage message, IMessageFunction<TArg, TResult> function, TArg arg)
            => function.Invoke<M, R>((M)message, arg);

        public ValueTask<TResult> InvokeAsync<TArg, TResult>(IMessage message, IMessageFunction<TArg, TResult> function, TArg arg)
            => function.InvokeAsync<M, R>((M)message, arg);
    }
}
