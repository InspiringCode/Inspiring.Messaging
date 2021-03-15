using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inspiring.Messaging.Core {
    public class MessagePipeline<M, R> : IMessagePipeline<M, R> where M : IMessage<M, R> {
        public static readonly MessagePipeline<M, R> Default = new();

        private readonly IResultAggregator<R> _defaultAggregator;
        private readonly Func<M, MessageContext, R> _processPipeline;
        private readonly Func<M, MessageContext, IEnumerable<IHandles<M, R>>> _getHandlerPipeline;
        private readonly Func<M, MessageContext, IEnumerable<IHandles<M, R>>, IEnumerable<R>> _dispatchPipeline;
        private readonly Func<M, MessageContext, IHandles<M, R>, R> _invokeHandlerPipeline;
        private readonly Func<M, MessageContext, IEnumerable<R>, R> _aggregatePipeline;

        public MessagePipeline()
            : this(Enumerable.Empty<IMessageMiddleware>()) { }

        public MessagePipeline(IEnumerable<IMessageMiddleware> middlewares)
            : this(middlewares, TakeLastResultAggregator<R>.Instance) { }

        public MessagePipeline(
            IEnumerable<IMessageMiddleware> middlewares,
            IResultAggregator<R> defaultAggregator
        ) {
            _defaultAggregator = defaultAggregator;

            _processPipeline = Pipeline<M, MessageContext, R>.Create(
                middlewares.OfType<IMessageProcessor<M, R>>(), 
                m => m.Process, 
                ProcessCore);

            _getHandlerPipeline = Pipeline<M, MessageContext, IEnumerable<IHandles<M, R>>>.Create(
                middlewares.OfType<IHandlerProvider<M, R>>(),
                m => m.GetHandlers,
                GetHandlers);

            _dispatchPipeline = Pipeline<M, MessageContext, IEnumerable<IHandles<M, R>>, IEnumerable<R>>.Create(
                middlewares.OfType<IMessageDispatcher<M, R>>(),
                m => m.Dispatch,
                DispatchToHandlers);

            _invokeHandlerPipeline = Pipeline<M, MessageContext, IHandles<M, R>, R>.Create(
                middlewares.OfType<IHandlerInvoker<M, R>>(),
                m => m.Invoke,
                InvokeHandler);

            _aggregatePipeline = Pipeline<M, MessageContext, IEnumerable<R>, R>.Create(
                middlewares.OfType<IMessageResultAggregator<M, R>>(),
                m => m.Aggregate,
                AggregateResults);
        }

        public R Process(M m, MessageContext context)
            => _processPipeline(m, context);

        protected virtual R ProcessCore(M m, MessageContext context) {
            IEnumerable<IHandles<M, R>> handlers = _getHandlerPipeline(m, context);
            IEnumerable<R> results = _dispatchPipeline(m, context, handlers);

            return _aggregatePipeline(m, context, results);
        }

        protected virtual IEnumerable<IHandles<M, R>> GetHandlers(M m, MessageContext context) =>
            context.Services.GetService<IEnumerable<IHandles<M, R>>>() ??
            Enumerable.Empty<IHandles<M, R>>();

        protected virtual IEnumerable<R> DispatchToHandlers(
            M m,
            MessageContext context,
            IEnumerable<IHandles<M, R>> handlers
        ) {
            return handlers.Select(h => _invokeHandlerPipeline.Invoke(m, context, h));
        }

        protected virtual R InvokeHandler(M m, MessageContext context, IHandles<M, R> h)
            => h.Handle(m);

        protected virtual R AggregateResults(M m, MessageContext context, IEnumerable<R> results)
            => _defaultAggregator.Aggregate(results);
    }
}
