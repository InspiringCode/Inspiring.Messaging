using System;
using System.Collections.Generic;
using System.Text;

namespace Inspiring.Messaging {
    public interface IHandles<M, R> where M : IMessage<M, R> {
    }
}
