using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiPerfDebugOverlay.Interfaces
{
    public interface INetworkProfiler
    {
        /// <summary>
        /// Enable/disable network profiling
        /// </summary>
        bool IsEnabled { get; set; }

        /// <summary>
        /// Total requests per second
        /// </summary>
        double RequestsPerSecond { get; }

        /// <summary>
        /// Total data transferred per second in bytes
        /// </summary>
        double BytesPerSecond { get; }


        /// <summary>
        /// Total data transferred since monitoring
        /// </summary>
        double TotalBytes { get; }

         
        /// <summary>
        /// Total requests since monitoring
        /// </summary>
        double TotalRequests { get; }


        /// <summary>
        /// Called every frame / second to update metrics
        /// </summary>
        void UpdateMetrics();
    }
}
