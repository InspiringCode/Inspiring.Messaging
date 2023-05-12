namespace Inspiring.Messaging.Pipelines {
    public readonly struct Invocation<M, R, O, C> {
        public readonly M Message;

        public readonly O Operation;

        public readonly C Context;

        public readonly Pipeline<M, R, O, C> Pipeline;

        public Invocation(M message, O operation, C context, Pipeline<M, R, O, C> pipeline) {
            Message = message;
            Operation = operation;
            Context = context;
            Pipeline = pipeline;
        }
    }
}
