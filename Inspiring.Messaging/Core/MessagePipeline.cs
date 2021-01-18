using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inspiring.Messaging.Core {
    public class MessagePipeline<M, R> : IMessagePipeline<M, R> where M : IMessage<M, R> {
        private readonly Func<IEnumerable<IHandles<M, R>>> _defaultHandlerFactory;
        private readonly Func<IResultAggregator<R>> _defaultAggregatorFactory;
        private readonly Func<M, PipelineParameters, R> _processPipeline;
        private readonly Func<M, PipelineParameters, IEnumerable<IHandles<M, R>>> _getHandlerPipeline;
        private readonly Func<M, PipelineParameters, IEnumerable<IHandles<M, R>>, IEnumerable<R>> _dispatchPipeline;
        private readonly Func<M, PipelineParameters, IHandles<M, R>, R> _invokeHandlerPipeline;
        private readonly Func<M, PipelineParameters, IEnumerable<R>, R> _aggregatePipeline;

        public MessagePipeline(
            IEnumerable<IMessageMiddleware<M, R>> middlewares, 
            Func<IEnumerable<IHandles<M, R>>> defaultHandlerFactory, 
            Func<IResultAggregator<R>> defaultAggregatorFactory
        ) {
            _defaultHandlerFactory = defaultHandlerFactory;
            _defaultAggregatorFactory = defaultAggregatorFactory;

            _processPipeline = Pipeline<M, PipelineParameters, R>.Create(
                middlewares.OfType<IMessageProcessor<M, R>>(), 
                m => m.Process, 
                ProcessCore);

            _getHandlerPipeline = Pipeline<M, PipelineParameters, IEnumerable<IHandles<M, R>>>.Create(
                middlewares.OfType<IHandlerProvider<M, R>>(),
                m => m.GetHandlers,
                GetHandlers);

            _dispatchPipeline = Pipeline<M, PipelineParameters, IEnumerable<IHandles<M, R>>, IEnumerable<R>>.Create(
                middlewares.OfType<IMessageDispatcher<M, R>>(),
                m => m.Dispatch,
                DispatchToHandlers);

            _invokeHandlerPipeline = Pipeline<M, PipelineParameters, IHandles<M, R>, R>.Create(
                middlewares.OfType<IHandlerInvoker<M, R>>(),
                m => m.Invoke,
                InvokeHandler);

            _aggregatePipeline = Pipeline<M, PipelineParameters, IEnumerable<R>, R>.Create(
                middlewares.OfType<IMessageResultAggregator<M, R>>(),
                m => m.Aggregate,
                AggregateResults);
        }

        public R Process(M m, PipelineParameters ps)
            => _processPipeline(m, ps);

        protected virtual R ProcessCore(M m, PipelineParameters ps) {
            IEnumerable<IHandles<M, R>> handlers = _getHandlerPipeline(m, ps);
            IEnumerable<R> results = _dispatchPipeline(m, ps, handlers);

            return _aggregatePipeline(m, ps, results);
        }

        protected virtual IEnumerable<IHandles<M, R>> GetHandlers(M m, PipelineParameters ps)
            => _defaultHandlerFactory();

        protected virtual IEnumerable<R> DispatchToHandlers(
            M m,
            PipelineParameters ps,
            IEnumerable<IHandles<M, R>> handlers
        ) {
            return handlers.Select(h => _invokeHandlerPipeline.Invoke(m, ps, h));
        }

        protected virtual R InvokeHandler(M m, PipelineParameters ps, IHandles<M, R> h)
            => h.Handle(m);

        protected virtual R AggregateResults(M m, PipelineParameters ps, IEnumerable<R> results)
            => _defaultAggregatorFactory().Aggregate(results);
    }
}
