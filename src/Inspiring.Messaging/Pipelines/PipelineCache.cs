using System;
using System.Collections.Generic;
using System.Text;

namespace Inspiring.Messaging.Pipelines;

public class PipelineCache : IPipelineCache {
    public static readonly PipelineCache DefaultCache = new();

    private readonly Dictionary<PipelineKey, object> _pipelines = new();

    public object Get(PipelineKey key) {
        _pipelines.TryGetValue(key, out object p);
        return p;
    }

    public object Set(PipelineKey key, Func<object> factory) {
        object p;

        lock (_pipelines) {
            if (!_pipelines.TryGetValue(key, out p)) {
                p = factory();
                _pipelines.Add(key, p);
            }
        }

        return p;
    }
}
