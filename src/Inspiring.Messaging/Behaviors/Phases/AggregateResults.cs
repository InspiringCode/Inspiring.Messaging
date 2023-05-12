namespace Inspiring.Messaging.Behaviors.Phases;

public readonly struct AggregateResults<R> {
    public readonly R[] Results;

    public readonly R? Result { get; init; }

    public AggregateResults(R[] results) {
        Results = results;
    }
}
