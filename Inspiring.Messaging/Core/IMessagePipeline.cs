using System;
using System.Collections.Generic;
using System.Text;

namespace Inspiring.Messaging.Core {
    public interface IMessagePipeline<M, R> where M : IMessage<M, R> {
        R Process(M m, MessageContext context);
    }
}
