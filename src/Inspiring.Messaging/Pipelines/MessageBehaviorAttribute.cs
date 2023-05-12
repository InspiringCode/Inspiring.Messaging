using System;
using System.Collections.Generic;
using System.Text;

namespace Inspiring.Messaging.Pipelines {
    public class MessageBehaviorAttribute : Attribute {
        public Type BehaviorType { get; }

        public MessageBehaviorAttribute(Type behaviorType)
            => BehaviorType = behaviorType ?? throw new ArgumentNullException(nameof(behaviorType));
    }
}
