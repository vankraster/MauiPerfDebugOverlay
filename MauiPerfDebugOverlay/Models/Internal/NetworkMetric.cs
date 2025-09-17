using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiPerfDebugOverlay.Models.Internal
{

    public class NetworkMetric
    {
        private static int _nextId = 0;
        public int Id { get; init; } = Interlocked.Increment(ref _nextId);
        public string Name { get; internal set; }
        public object Value { get; internal set; }
        public KeyValuePair<string, object?>[] Tags { get; internal set; }
        public DateTime Timestamp { get; internal set; }
        public bool IsExpanded { get; set; } = false;
    }
}
