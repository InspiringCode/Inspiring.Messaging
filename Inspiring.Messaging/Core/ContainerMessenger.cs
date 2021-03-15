using System;
using System.Collections.Generic;
using System.Text;

namespace Inspiring.Messaging.Core {
    public class ContainerMessenger : IMessenger {
        private readonly IServiceProvider _services;

        public ContainerMessenger(IServiceProvider services)
            => _services = services;

        public R Send<M, R>(IMessage<M, R> message) where M : IMessage<M, R> {
            IMessagePipeline<M, R> pipeline = 
                _services.GetService<IMessagePipeline<M, R>>() ?? 
                MessagePipeline<M, R>.Default;

            return pipeline.Process((M)message, new MessageContext(_services));
        }
    }
}
