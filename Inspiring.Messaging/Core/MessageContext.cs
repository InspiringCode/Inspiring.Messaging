using System;
using System.Collections.Generic;
using System.Text;

namespace Inspiring.Messaging.Core {
    public readonly struct MessageContext {
        private static readonly IServiceProvider NullProvider = new NullServiceProvider();
        private readonly IServiceProvider? _services;

        public MessageContext(IServiceProvider services) {
            _services = services;
        }

        public IServiceProvider Services
            => _services ?? NullProvider;

        private class NullServiceProvider : IServiceProvider {
            public object? GetService(Type serviceType) => null;
        }
    }
}
