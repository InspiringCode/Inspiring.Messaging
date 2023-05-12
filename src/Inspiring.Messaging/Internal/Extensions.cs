using System;
using System.Threading;

namespace Inspiring.Messaging {
    internal static class Extensions {
        public static bool IsCritical(this Exception ex) =>
            ex is AccessViolationException ||
            ex is OutOfMemoryException ||
            ex is StackOverflowException ||
            ex is ThreadAbortException ||
            ex is AppDomainUnloadedException ||
            ex is BadImageFormatException ||
            ex is CannotUnloadAppDomainException ||
            ex is InvalidProgramException;
    }
}
