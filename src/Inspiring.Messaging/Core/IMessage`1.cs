using System;
using System.Collections.Generic;
using System.Text;

namespace Inspiring.Messaging.Core {
    public interface IMessage { }

    public interface IMessage<out M> : IMessage { }
}
