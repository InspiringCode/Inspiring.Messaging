using System;

namespace Inspiring.Messaging.Pipelines;

public interface IPipelineCache {
    object Get(PipelineKey key);

    object Set(PipelineKey key, Func<object> factory);
}

public struct PipelineKey : IEquatable<PipelineKey> {
    public readonly Type MessageType;
    public readonly Type OperationType;
    public readonly Type ContextType;

    public PipelineKey(Type messageType, Type operationType, Type conextType) : this() {
        MessageType = messageType;
        ContextType = conextType;
        OperationType = operationType;
    }

    public bool Equals(PipelineKey other) =>
        MessageType == other.MessageType &&
        OperationType == other.OperationType &&
        ContextType == other.ContextType;

    public override bool Equals(object? other)
        => other is PipelineKey o && Equals(o);

    public override int GetHashCode()
        => HashCode.Combine(MessageType, OperationType, ContextType);

    public override string? ToString()
        => MessageType?.Name;
}
