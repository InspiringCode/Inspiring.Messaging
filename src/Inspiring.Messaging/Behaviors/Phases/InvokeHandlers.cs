using Inspiring.Messaging.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Inspiring.Messaging.Behaviors.Phases;
public readonly struct InvokeHandlers<R> {
    private static readonly R[] EmptyResults = new R[0];

    public readonly IHandler[] Handlers;

    public readonly R?[] Results { get; init; }

    public InvokeHandlers(IHandler[] handlers) {
        Handlers = handlers;
        Results = EmptyResults;
    }
}
