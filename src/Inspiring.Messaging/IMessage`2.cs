using System;
using System.Collections.Generic;
using System.Text;

namespace Inspiring.Messaging {
    public interface IMessage<out M, in R> where M : IMessage<M, R> { }
}
