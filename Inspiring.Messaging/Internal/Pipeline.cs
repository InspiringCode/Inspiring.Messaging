using System;
using System.Collections.Generic;
using System.Linq;

namespace Inspiring.Messaging {
    internal static class Pipeline<T1, TRet> {
        public static Func<T1, TRet> Create<TMiddleware>(
            IEnumerable<TMiddleware> steps,
            Func<TMiddleware, Func<T1, Func<T1, TRet>, TRet>> middlewareFuncSelector,
            Func<T1, TRet> last
        ) {
            return steps
                .Select(middlewareFuncSelector)
                .Aggregate(last, (next, mw) => {
                    return (p1) => mw(p1, next);
                });
        }
    }

    internal static class Pipeline<T1, T2, TRet> {
        public static Func<T1, T2, TRet> Create<TMiddleware>(
            IEnumerable<TMiddleware> steps,
            Func<TMiddleware, Func<T1, T2, Func<T1, T2, TRet>, TRet>> middlewareFuncSelector,
            Func<T1, T2, TRet> last
        ) {
            return steps
                .Select(middlewareFuncSelector)
                .Aggregate(last, (next, mw) => {
                    return (p1, p2) => mw(p1, p2, next);
                });
        }
    }

    internal static class Pipeline<T1, T2, T3, TRet> {
        public static Func<T1, T2, T3, TRet> Create<TMiddleware>(
            IEnumerable<TMiddleware> steps,
            Func<TMiddleware, Func<T1, T2, T3, Func<T1, T2, T3, TRet>, TRet>> middlewareFuncSelector,
            Func<T1, T2, T3, TRet> last
        ) {
            return steps
                .Select(middlewareFuncSelector)
                .Aggregate(last, (next, mw) => {
                    return (p1, p2, p3) => mw(p1, p2, p3, next);
                });
        }
    }
}
