using System;
using System.Collections.Generic;
using System.Text;

namespace Inspiring.Messaging.Pipelines.Internal {
    internal class NullServiceProvider : IServiceProvider {
        public static readonly NullServiceProvider Instance = new();

        public object? GetService(Type serviceType) => null;
    }
}
