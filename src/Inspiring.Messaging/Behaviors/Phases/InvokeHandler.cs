using Inspiring.Messaging.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inspiring.Messaging.Behaviors.Phases;

public struct InvokeHandler<R> {
    public readonly IHandler Handler;
    
    public readonly R? Result { get; init; }

    public InvokeHandler(IHandler handler) : this() {
        Handler = handler;
        Result = default;
    }
}
