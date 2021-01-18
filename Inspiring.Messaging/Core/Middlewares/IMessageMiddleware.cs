using System;
using System.Collections.Generic;
using System.Text;

namespace Inspiring.Messaging.Core {
    public interface IMessageMiddleware<M, R> where M : IMessage<M, R> {
    }
}
