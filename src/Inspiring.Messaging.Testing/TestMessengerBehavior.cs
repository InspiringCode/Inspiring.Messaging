using Inspiring.Messaging.Behaviors.Phases;
using Inspiring.Messaging.Pipelines;
using System;
using System.Threading.Tasks;

namespace Inspiring.Messaging.Testing;

public class TestMessengerBehavior : IMessageBehavior {
    private readonly TestMessageHandler _handler = new();

    public void Configure<M, R, O, C>(PipelineBuilder<M, R, O, C> pipeline) {
        TestMessageHandler.AddToPipeline(pipeline, _handler);
    }
}
