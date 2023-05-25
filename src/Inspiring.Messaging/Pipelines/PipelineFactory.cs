using Inspiring.Messaging.Behaviors;
using Inspiring.Messaging.Behaviors.Phases;
using Inspiring.Messaging.Pipelines.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Inspiring.Messaging.Pipelines
{
    public class PipelineFactory {
        public static readonly PipelineFactory DefaultFactory = new();

        private readonly IServiceProvider _behavorFactory;
        private readonly IMessageBehavior[] _generalBehaviors;

        public PipelineFactory()
            : this(Array.Empty<IMessageBehavior>()) { }

        public PipelineFactory(IEnumerable<IMessageBehavior> generalBehaviors)
            : this(generalBehaviors, NullServiceProvider.Instance) { }

        public PipelineFactory(IEnumerable<IMessageBehavior> generalBehaviors, IServiceProvider behaviorFactory) {
            _generalBehaviors = generalBehaviors.ToArray();
            _behavorFactory = behaviorFactory;
        }

        public virtual Pipeline<M, R, O, C> Create<M, R, O, C>() where M : IMessage<M, R> {
            PipelineBuilder<M, R, O, C> builder = new();

            builder.SetTerminalStep(new DefaultMessageProcessor<M, R, O, C>());
            builder.SetTerminalStep(new DefaultHandlersInvoker<M, R, O, C>());
            builder.SetTerminalStep(new DefaultHandlerInvoker<M, R, O, C>());
            builder.SetTerminalStep(new FirstOrDefaultResultAggregator<M, R, O, C>());

            if (typeof(IServiceProviderContext).IsAssignableFrom(typeof(C))) {
                builder.SetTerminalStep((PipelineStep<M, R, O, C, ProvideHandlers>)typeof(ContainerHandlerProvider<,,,>)
                    .MakeGenericType(typeof(M), typeof(R), typeof(O), typeof(C))
                    .GetConstructor(Type.EmptyTypes)
                    .Invoke(null));
            }

            ConfigurePipeline(builder);
            return builder.Build();
        }

        protected virtual void ConfigurePipeline<M, R, O, C>(PipelineBuilder<M, R, O, C> b) {
            IEnumerable<IMessageBehavior> behaviors = _generalBehaviors
                .Concat(new[] { typeof(R), typeof(M), typeof(C), typeof(O) }
                    .SelectMany(
                        t => t.GetCustomAttributes<MessageBehaviorAttribute>(inherit: true),
                        (t, attribute) => CreateBehavior(attribute.BehaviorType)));

            foreach (IMessageBehavior behavior in behaviors)
                behavior.Configure(b);
        }

        private IMessageBehavior CreateBehavior(Type behaviorType) {
            return (IMessageBehavior)
                (_behavorFactory.GetService(behaviorType) ??
                createWithReflection(behaviorType));

            static object createWithReflection(Type behaviorType) {
                try {
                    return Activator.CreateInstance(behaviorType, nonPublic: true);
                } catch (Exception e) when (!e.IsCritical()) {
                    throw new ArgumentException($"The configured message behavior ('{behaviorType.Name}') " +
                        $"can not be resolved or created. Make sure that it either has a parameterless constructor " +
                        $"or that you provide an 'IServiceProvider' to the messenger (or pipeline factory) that " +
                        $"returns an instance for the behavior type. See inner exception for further details.", e);
                }
            }
        }
    }
}
