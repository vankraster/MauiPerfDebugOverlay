namespace MauiPerfDebugOverlay.Models
{
    public class PerformanceOverlayOptions
    {
        /// <summary>
        /// Only in Android! show battery usage in mW.
        /// </summary>
        public bool ShowBatteryUsage { get; set; } = false;

        /// <summary>
        /// Monitoring Network stats (requests count, bytes sent/received). only if httpclient not webrequest !
        /// </summary>
        public bool ShowNetworkStats { get; set; } = false;

        /// <summary>
        /// Show GC allocations (collections count, total memory)   And Allocation
        /// </summary>
        public bool ShowAlloc_GC { get; set; } = true;
         
        /// <summary>
        /// Show CPU Usage (total, app) && threads
        /// </summary>
        public bool ShowCPU_Usage{ get; set; } = true;

        /// <summary>
        /// Show memory usage (total, app)
        /// </summary>
        public bool ShowMemory { get; set; } = false;
         
        /// <summary>
        /// Show Frames per second (FPS) and Frame time (ms)    
        /// </summary>
        public bool ShowFrame { get; set; } = true;
    }
}
