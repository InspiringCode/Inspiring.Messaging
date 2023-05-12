using System;

namespace Inspiring.Messaging.Pipelines;
public struct DefaultContext : IServiceProviderContext {
    public IServiceProvider OperationServices { get; }

    public DefaultContext(IServiceProvider operationServices) {
        OperationServices = operationServices ?? throw new ArgumentNullException(nameof(operationServices));
    }
}
