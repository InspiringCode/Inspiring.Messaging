using System;
using System.Collections.Generic;
using System.Text;

namespace Inspiring.Messaging {
    public interface IHandles<in M, out R> where M : IMessage<M, R> {
        R Handle(M m);
    }
}
