using Inspiring.Messaging.Core;

namespace Inspiring.Messaging.Behaviors.Phases;

public readonly struct ProvideHandlers {
    public static readonly ProvideHandlers Empty = new(new IHandler[0]);

    public readonly IHandler[] Handlers { get; init; }

    public ProvideHandlers(IHandler[] handler) {
        Handlers = handler;
    }
}
