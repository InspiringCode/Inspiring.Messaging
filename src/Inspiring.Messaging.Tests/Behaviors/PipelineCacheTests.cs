using System.Collections.Generic;

namespace Inspiring.Messaging.Pipelines;

public class PipelineCacheTests : FeatureBase {
    [Scenario]
    internal void GetAndSet(
        PipelineCache cache,
        object first,
        object actual,
        PipelineKey key
    ) {
        GIVEN["a cache"] = () => cache = new PipelineCache();

        WHEN["getting a pipeline not yet chached"] = () => actual = cache.Get(key = new(typeof(Message1), typeof(object), typeof(object)));
        THEN["null is returned"] = () => actual.Should().BeNull();

        WHEN["setting a pipeline for a key"] = () => actual = cache.Set(key, () => first == null ? (first = new object()) : new object()); // create only on first invocation
        THEN["the created object is returned"] = () => actual.Should().Be(first);

        WHEN["setting a pipeline for a key a second time"] = () => actual = cache.Set(key, () => new object());
        THEN["the first pipeline instance is returned"] = () => actual.Should().Be(first);

        WHEN["getting a cached pipeline"] = () => actual = cache.Get(key);
        THEN["the cached instance is returned"] = () => actual.Should().Be(first);
    }

    internal record Message1;
}
