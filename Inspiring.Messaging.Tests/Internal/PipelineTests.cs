using FluentAssertions;
using Inspiring.Messaging.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using Xbehave;

namespace Inspiring.Messaging.Tests.Internal {
    public class PipelineTests : Feature {
        [Scenario]
        internal void Creation(Middleware[] mws, Func<string, IEnumerable<string>> pipeline, Func<string, IEnumerable<string>> last, IEnumerable<string> result) {
            GIVEN["some middlewares"] |= () => mws = new[] {
                new Middleware { Value = "M1" },
                new Middleware { Value = "M2" },
                new Middleware { Value = "M3" }
            };
            AND["a last function"] |= () => last = param => new[] { param };
            AND["a pipeline"] |= () => pipeline = Pipeline<string, IEnumerable<string>>.Create(mws, mw => mw.Invoke, last);

            WHEN["invoking the pipeline"] |= () => result = pipeline("P");
            THEN["all steps are invoked"] |= () => result.Should().BeEquivalentTo("P", "P:M1", "P:M2", "P:M3");

            GIVEN["an empty pipeline"] |= () => pipeline = Pipeline<string, IEnumerable<string>>.Create(new Middleware[0], mw => mw.Invoke, last);
            WHEN["invoking the pipeline"] |= () => result = pipeline("P");
            THEN["only the last function is called"] |= () => result.Should().BeEquivalentTo("P");
        }

        internal class Middleware {
            public string Value { get; init; }

            public IEnumerable<string> Invoke(string param, Func<string, IEnumerable<string>> next) {
                return next(param).Concat(new[] { $"{param}:{Value}" });
            }
        }
    }
}
