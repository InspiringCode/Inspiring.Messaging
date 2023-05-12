using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Inspiring.Messaging {
    public class TestServiceProvider : IServiceProvider, IEnumerable {
        private readonly List<object> _services;

        public TestServiceProvider(IEnumerable<object> services = null)
            => _services = services?.ToList() ?? new();

        public void Add(object instance)
            => _services.Add(instance);

        public void Clear()
            => _services.Clear();

        public IEnumerator GetEnumerator()
            => _services.GetEnumerator();

        public object GetService(Type serviceType) { 
            if (serviceType.Name == "IEnumerable`1") {
                serviceType = serviceType.GenericTypeArguments[0];

                object[] instances = _services
                    .Where(x => serviceType.IsAssignableFrom(x.GetType()))
                    .ToArray();

                Array result = Array.CreateInstance(serviceType, instances.Length);
                instances.CopyTo(result, 0);
                return result;
            }

            return _services.FirstOrDefault(x => serviceType.IsAssignableFrom(x.GetType()));
        }
    }
}
