using System;
using System.Collections.Generic;
using System.Text;

namespace Inspiring.Messaging.Core {
    public class ContainerMessenger : IMessenger {
        private readonly IServiceProvider _services;

        public ContainerMessenger(IServiceProvider services)
            => _services = services;

        public R Send<M, R>(IMessage<M, R> message) where M : IMessage<M, R> {
            var pipeline = (IMessagePipeline<M, R>)_services.GetService(typeof(IMessagePipeline<M, R>));
            return pipeline.Process((M)message, new MessageContext(_services));
        }
    }
}
