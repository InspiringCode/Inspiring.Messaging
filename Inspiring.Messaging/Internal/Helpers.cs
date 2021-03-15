using System;
using System.Collections.Generic;
using System.Text;

namespace Inspiring.Messaging {
    internal static class Helpers {
        public static T? GetService<T>(this IServiceProvider services)
            => (T?)services.GetService(typeof(T));
    }
}
