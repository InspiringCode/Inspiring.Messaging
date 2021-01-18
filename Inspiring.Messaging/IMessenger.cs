using System;
using System.Collections.Generic;
using System.Text;

namespace Inspiring.Messaging {
    public interface IMessenger {
        R Send<M, R>(IMessage<M, R> message) where M : IMessage<M, R>;
    }
}
