using System;
using System.Collections.Generic;
using System.Text;

namespace Inspiring.Messaging.Pipelines {
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct, AllowMultiple = true)]
    public class MessageBehaviorAttribute : Attribute {
        public Type BehaviorType { get; }

        public MessageBehaviorAttribute(Type behaviorType)
            => BehaviorType = behaviorType ?? throw new ArgumentNullException(nameof(behaviorType));
    }
}
