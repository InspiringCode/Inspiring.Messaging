using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inspiring.Messaging {
    public class TestContainer : IServiceProvider, IEnumerable {
        private readonly List<object> _services = new();

        public TestContainer Add(object service) {
            _services.Add(service);
            return this;
        }

        public IEnumerator GetEnumerator()
            => throw new NotSupportedException();

        public object GetService(Type serviceType) {
            return _services
                .Where(s => serviceType.IsAssignableFrom(s.GetType()))
                .LastOrDefault();
        }
    }
}
