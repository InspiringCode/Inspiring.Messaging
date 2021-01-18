﻿using System;

namespace Inspiring.Messaging.Core {
    public interface IMessageProcessor<M, R> where M : IMessage<M, R> {
        R Process(M m, PipelineParameters ps, Func<M, PipelineParameters, R> next);
    }
}