using Inspiring.Messaging.Behaviors;
using Inspiring.Messaging.Pipelines;

namespace Inspiring.Messaging.Core;

[MessageBehavior(typeof(SendBehavior))]
public class SendOperation {
    public static readonly SendOperation Instance = new();
}
