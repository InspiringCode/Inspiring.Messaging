using System;

namespace Inspiring.Messaging.Pipelines;

public interface IServiceProviderContext {
    IServiceProvider OperationServices { get; }
}
