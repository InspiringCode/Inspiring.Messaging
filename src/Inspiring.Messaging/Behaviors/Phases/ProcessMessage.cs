namespace Inspiring.Messaging.Behaviors.Phases;

public readonly struct ProcessMessage<R> {
    public readonly R? Result;

    public ProcessMessage(R? result) {
        Result = result;
    }
}
