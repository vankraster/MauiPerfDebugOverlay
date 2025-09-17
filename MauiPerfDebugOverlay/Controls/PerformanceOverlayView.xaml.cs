
using MauiPerfDebugOverlay.Enums;
using MauiPerfDebugOverlay.Extensions;
using MauiPerfDebugOverlay.Interfaces;
using MauiPerfDebugOverlay.Models.Internal;
using MauiPerfDebugOverlay.Platforms;
using MauiPerfDebugOverlay.Services;
using System.Diagnostics;
namespace MauiPerfDebugOverlay.Controls
{
    public partial class PerformanceOverlayView : ContentView
    {
        private readonly IFpsService _fpsService;
        private TState CurrentState = TState.TabGeneral;

        private double _overallScore;
        private double _cpuUsage;
        private TimeSpan _prevCpuTime;
        private double _memoryUsage;
        private int _threadCount;
        private int _processorCount = Environment.ProcessorCount;


        private volatile bool _stopRequested = false;
        private Stopwatch _stopwatch;


        //variabile cara calculeaza fps cu EMA (Exponential Moving Average)
        private double _emaFrameTime = 0;
        private double _emaFps = 0;
        private const double _emaAlpha = 0.9;


        //hitch
        private const double HitchThresholdMs = 200;
        private double _emaHitch = 0;
        private double _emaHighestHitch = 0;
        private const double _emaHitchAlpha = 0.7; // mai reactiv decât FPS/FrameTime


        //GC
        private int _gc0Prev = 0;
        private int _gc1Prev = 0;
        private int _gc2Prev = 0;

        private int _gc0Delta = 0;
        private int _gc1Delta = 0;
        private int _gc2Delta = 0;


        //Alloc/sec
        private long _lastTotalMemory = 0;
        private double _allocPerSec = 0;

        private readonly Process _currentProcess = Process.GetCurrentProcess();
        private long _lastAllocatedBytes = GC.GetTotalAllocatedBytes(false);

        //Networking
        long totalRequests = 0;
        long totalSent = 0;
        long totalReceived = 0;

        //double totalRequestsPerSecond = 0;
        //double totalSentPerSecond = 0;
        //double totalReceivedPerSecond = 0;

        double avgRequestTime = 0;

        //overall score
        private double _emaOverallScore = 0;
        private const double _emaOverallAlpha = 0.6; // 0–1, mai mare = mai reactiv

        private double _batteryMilliW = 0;
        private bool _batteryMilliWAvailable = true;


        private DumpCurrentPageService _dumpService = new DumpCurrentPageService();

        public PerformanceOverlayView()
        {
            InitializeComponent();

            _stopwatch = new Stopwatch();

            showUiItems();

            if (PerformanceDebugOverlayExtensions.PerformanceOverlayOptions.ShowFrame)
            {
                _fpsService = new FpsService();
                _fpsService.OnFrameTimeCalculated += frameTimeMs =>
                {
                    const double MinFrameTime = 0.1; // ms, pentru a evita diviziunea la zero
                    frameTimeMs = Math.Max(frameTimeMs, MinFrameTime);

                    // EMA FrameTime
                    if (_emaFrameTime == 0)
                        _emaFrameTime = frameTimeMs;
                    else
                        _emaFrameTime = (_emaAlpha * _emaFrameTime) + ((1 - _emaAlpha) * frameTimeMs);

                    // EMA FPS
                    double fps = 1000.0 / frameTimeMs;
                    if (_emaFps == 0)
                        _emaFps = fps;
                    else
                        _emaFps = (_emaAlpha * _emaFps) + ((1 - _emaAlpha) * fps);

                    // Hitch EMA
                    double hitchValue = frameTimeMs >= HitchThresholdMs ? frameTimeMs : 0;
                    if (_emaHitch == 0)
                        _emaHitch = hitchValue;
                    else
                        _emaHitch = (_emaHitchAlpha * _emaHitch) + ((1 - _emaHitchAlpha) * hitchValue);

                    if (_emaHitch > _emaHighestHitch)
                        _emaHighestHitch = _emaHitch;

                };
            }
        }

        private void showUiItems()
        {
            //BoxViewNetwork.IsVisible = NetworkLabel.IsVisible = PerformanceDebugOverlayExtensions.PerformanceOverlayOptions.ShowNetworkStats;
            BoxViewNetwork.IsVisible = NetworkLabel.IsVisible = false;
            BoxViewBattery.IsVisible = BatteryLabel.IsVisible = PerformanceDebugOverlayExtensions.PerformanceOverlayOptions.ShowBatteryUsage;
            FpsLabel.IsVisible = FrameTimeLabel.IsVisible = HitchLabel.IsVisible = HighestHitchLabel.IsVisible = PerformanceDebugOverlayExtensions.PerformanceOverlayOptions.ShowFrame;
            BoxViewCpu.IsVisible = CpuLabel.IsVisible = ThreadsLabel.IsVisible = PerformanceDebugOverlayExtensions.PerformanceOverlayOptions.ShowCPU_Usage;

            BoxViewMemory_GC.IsVisible = PerformanceDebugOverlayExtensions.PerformanceOverlayOptions.ShowAlloc_GC || PerformanceDebugOverlayExtensions.PerformanceOverlayOptions.ShowMemory;
            GcLabel.IsVisible = AllocLabel.IsVisible = PerformanceDebugOverlayExtensions.PerformanceOverlayOptions.ShowAlloc_GC;
            MemoryLabel.IsVisible = PerformanceDebugOverlayExtensions.PerformanceOverlayOptions.ShowMemory;
        }





        public void Start()
        {
            _stopRequested = false;
            _fpsService?.Start();
            StartMetrics();
        }
        public void Stop()
        {
            _fpsService?.Stop();
            _stopRequested = true;
        }

        private void UpdateExtraMetrics()
        {
            if (PerformanceDebugOverlayExtensions.PerformanceOverlayOptions.ShowBatteryUsage)
                UpdateBatteryUsage();

            //if (PerformanceDebugOverlayExtensions.PerformanceOverlayOptions.ShowNetworkStats)
            //    UpdateNetworkStats();

        }



        private void UpdateNetworkStats()
        {
            //var profiler = NetworkProfiler.Instance;

            //totalRequests = profiler.TotalRequests;
            //totalSent = profiler.TotalBytesSent;
            //totalReceived = profiler.TotalBytesReceived;
            //avgRequestTime = profiler.AverageRequestTimeMs;

            //totalReceivedPerSecond = profiler.BytesReceivedPerSecond;
            //totalSentPerSecond = profiler.BytesSentPerSecond;
            //totalRequestsPerSecond = profiler.RequestsPerSecond;
        }

        private void UpdateBatteryUsage()
        {
#if ANDROID
            try
            {
                _batteryMilliW = BatteryService.GetBatteryMilliW();
                _batteryMilliWAvailable = true;
            }
            catch
            {
                _batteryMilliW = 0;
                _batteryMilliWAvailable = false;
            }
#else
            _batteryMilliW = 0;
            _batteryMilliWAvailable = false;
#endif
        }



        private void UpdateGcAndAllocMetrics()
        {
            double elapsedSec = _stopwatch.Elapsed.TotalSeconds;
            if (elapsedSec <= 0) elapsedSec = 1; // fallback

            // Alloc/sec
            long currentAllocated = GC.GetTotalAllocatedBytes(false);
            long deltaAllocated = currentAllocated - _lastAllocatedBytes;
            _allocPerSec = (deltaAllocated / (1024.0 * 1024.0)) / elapsedSec; // MB/sec
            _lastAllocatedBytes = currentAllocated;

            // GC counts
            int gen0 = GC.CollectionCount(0);
            int gen1 = GC.CollectionCount(1);
            int gen2 = GC.CollectionCount(2);

            _gc0Delta = gen0 - _gc0Prev;
            _gc1Delta = gen1 - _gc1Prev;
            _gc2Delta = gen2 - _gc2Prev;

            _gc0Prev = gen0;
            _gc1Prev = gen1;
            _gc2Prev = gen2;

        }
        private void UpdateUi()
        {
            if (PerformanceDebugOverlayExtensions.PerformanceOverlayOptions.ShowFrame)
            {
                FrameTimeLabel.Text = $"FrameTime: {_emaFrameTime:F1} ms";
                FrameTimeLabel.TextColor = _emaFrameTime <= 16 ? Color.FromHex("7CBF8E") :
                                           _emaFrameTime <= 33 ? Color.FromHex("FFECB3") : Color.FromHex("D98880");

                FpsLabel.Text = $"FPS: {_emaFps:F1}";
                FpsLabel.TextColor = _emaFps >= 50 ? Color.FromHex("7CBF8E") :
                                     _emaFps >= 30 ? Color.FromHex("FFECB3") : Color.FromHex("D98880");

                if (_emaHitch >= HitchThresholdMs)
                {
                    HitchLabel.Text = $"Last Hitch: {_emaHitch:F0} ms";
                    HitchLabel.TextColor = Color.FromHex("D98880");

                    HighestHitchLabel.Text = $"Highest Hitch: {_emaHighestHitch:F0} ms";
                    HighestHitchLabel.TextColor = Color.FromHex("D98880");
                }
            }

            if (PerformanceDebugOverlayExtensions.PerformanceOverlayOptions.ShowAlloc_GC)
            {
                AllocLabel.Text = $"Alloc/sec: {_allocPerSec:F2} MB";
                AllocLabel.TextColor = _allocPerSec < 5 ? Color.FromHex("7CBF8E") :
                                       _allocPerSec < 10 ? Color.FromHex("FFECB3") : Color.FromHex("D98880");

                GcLabel.Text = $"GC: Gen0 {_gc0Delta}, Gen1 {_gc1Delta}, Gen2 {_gc2Delta}";
                GcLabel.TextColor = (_gc0Delta + _gc1Delta + _gc2Delta) == 0
                    ? Color.FromHex("7CBF8E") : Color.FromHex("FFECB3");
            }

            if (PerformanceDebugOverlayExtensions.PerformanceOverlayOptions.ShowMemory)
            {
                MemoryLabel.Text = $"Memory: {_memoryUsage} MB";
                MemoryLabel.TextColor = _memoryUsage < 260 ? Color.FromHex("7CBF8E") :
                                        _memoryUsage < 400 ? Color.FromHex("FFECB3") : Color.FromHex("D98880");
            }

            if (PerformanceDebugOverlayExtensions.PerformanceOverlayOptions.ShowCPU_Usage)
            {
                ThreadsLabel.Text = $"Threads: {_threadCount}";
                ThreadsLabel.TextColor = _threadCount < 50 ? Color.FromHex("7CBF8E") :
                                         _threadCount < 100 ? Color.FromHex("FFECB3") : Color.FromHex("D98880");

                CpuLabel.Text = $"CPU: {_cpuUsage:F1}%";
                CpuLabel.TextColor = _cpuUsage < 30 ? Color.FromHex("7CBF8E") :
                                     _cpuUsage < 60 ? Color.FromHex("FFECB3") : Color.FromHex("D98880");
            }

            if (PerformanceDebugOverlayExtensions.PerformanceOverlayOptions.ShowBatteryUsage)
            {
                if (_batteryMilliWAvailable)
                {
                    BatteryLabel.Text = $"Battery consumption: {_batteryMilliW:F1} mW";
                    BatteryLabel.TextColor = _batteryMilliW < 100 ? Color.FromHex("7CBF8E") :
                                             _batteryMilliW < 500 ? Color.FromHex("FFECB3") : Color.FromHex("D98880");
                }
                else
                {
                    BatteryLabel.Text = "Battery consumption: N/A";
                    BatteryLabel.TextColor = Colors.Gray;
                }
            }

            //if (PerformanceDebugOverlayExtensions.PerformanceOverlayOptions.ShowNetworkStats)
            //{
            //    NetworkLabel.Text =
            //    $"Requests: {totalRequests}\n" +
            //    $"Sent: {totalSent / 1024.0:F1} KB\n" +
            //    $"Received: {totalReceived / 1024.0:F1} KB\n" +
            //    $"Avg Req. Time: {avgRequestTime:F1} ms\n";
            //    //$"Requests per sec.: {totalRequestsPerSecond}\n" +
            //    //$"Sent per sec.: {totalSentPerSecond / 1024.0:F1} KB\n" +
            //    //$"Received per sec.: {totalReceivedPerSecond / 1024.0:F1} KB";
            //}

            //ScoreLabel.Text = $"Overall: {_emaOverallScore:F1}/10";
            //ScoreLabel.TextColor = _emaOverallScore >= 8 ? Color.FromHex("7CBF8E") :
            //                       _emaOverallScore >= 5 ? Color.FromHex("FFECB3") : Color.FromHex("D98880");
        }


        private void StartMetrics()
        {
            _stopwatch.Restart();
            _prevCpuTime = _currentProcess.TotalProcessorTime;

            Microsoft.Maui.Controls.Application.Current!.Dispatcher.StartTimer(TimeSpan.FromSeconds(1), () =>
            {
                if (PerformanceDebugOverlayExtensions.PerformanceOverlayOptions.ShowAlloc_GC)
                    UpdateGcAndAllocMetrics();

                UpdateExtraMetrics();

                if (PerformanceDebugOverlayExtensions.PerformanceOverlayOptions.ShowCPU_Usage)
                {
                    _memoryUsage = _currentProcess.WorkingSet64 / (1024 * 1024);
                    _threadCount = _currentProcess.Threads.Count;

                    var currentCpuTime = _currentProcess.TotalProcessorTime;
                    double cpuDelta = (currentCpuTime - _prevCpuTime).TotalMilliseconds;

                    double interval = _stopwatch.Elapsed.TotalMilliseconds; // ⬅️ real interval
                    _cpuUsage = (cpuDelta / interval) * 100 / _processorCount;

                    _prevCpuTime = currentCpuTime;
                }
                _stopwatch.Restart();

                //_overallScore = CalculateOverallScore();
                //UpdateOverallScore(_overallScore);

                MainThread.BeginInvokeOnMainThread(UpdateUi);

                return !_stopRequested;
            });
        }

        private double CalculateOverallScore()
        {
            double score = 0;

            // FPS (max 3 puncte)
            if (_emaFps >= 50) score += 3;
            else if (_emaFps >= 30) score += 2;
            else score += 1;

            // CPU (max 3 puncte)
            if (_cpuUsage < 30) score += 3;
            else if (_cpuUsage < 60) score += 2;
            else score += 1;

            // Memory (max 2 puncte)
            if (_memoryUsage < 260) score += 2;
            else if (_memoryUsage < 400) score += 1;
            // >400 → 0

            // Threads (max 2 puncte)
            if (_threadCount < 50) score += 2;
            else if (_threadCount < 100) score += 1;
            // >100 → 0

            return score; // max 10
        }

        private void UpdateOverallScore(double rawScore)
        {

            if (_emaOverallScore == 0)
                _emaOverallScore = rawScore;
            else
                _emaOverallScore = (_emaOverallAlpha * _emaOverallScore) + ((1 - _emaOverallAlpha) * rawScore);
        }


        #region Drag && Move

        private const int HistorySize = 5;
        private const double MovementThreshold = 1.0;

        private readonly Queue<double> _totalXHistory = new();
        private readonly Queue<double> _totalYHistory = new();
        private double _lastAverageX;
        private double _lastAverageY;
        private double _currentXPosition;
        private double _currentYPosition;

        private void OnPanUpdated(object? sender, PanUpdatedEventArgs e)
        {
            if (CurrentState == TState.TabTree)
                return;
            if (sender is not Frame frame) return;
            if (frame.Parent is not PerformanceOverlayView overlay) return;
            if (overlay.Parent is not AbsoluteLayout parent) return;

            switch (e.StatusType)
            {
                case GestureStatus.Started:
                    var bounds = AbsoluteLayout.GetLayoutBounds(overlay);
                    _currentXPosition = bounds.X;
                    _currentYPosition = bounds.Y;

                    _totalXHistory.Clear();
                    _totalXHistory.Enqueue(e.TotalX);
                    _lastAverageX = e.TotalX;

                    _totalYHistory.Clear();
                    _totalYHistory.Enqueue(e.TotalY);
                    _lastAverageY = e.TotalY;
                    break;

                case GestureStatus.Running:
                    #region X Axis
                    _totalXHistory.Enqueue(e.TotalX);
                    if (_totalXHistory.Count > HistorySize) _totalXHistory.Dequeue();

                    double currentAverageX = _totalXHistory.Average();
                    double deltaX = currentAverageX - _lastAverageX;

                    if (Math.Abs(deltaX) >= MovementThreshold)
                    {
                        double newX = _currentXPosition + deltaX;
                        newX = Math.Max(0, newX);
                        newX = Math.Min(parent.Width - overlay.Width, newX);
                        _currentXPosition = newX;

                        var boundsX = AbsoluteLayout.GetLayoutBounds(overlay);
                        boundsX.X = newX;
                        AbsoluteLayout.SetLayoutBounds(overlay, boundsX);

                        _lastAverageX = currentAverageX;
                    }
                    #endregion

                    #region Y Axis
                    _totalYHistory.Enqueue(e.TotalY);
                    if (_totalYHistory.Count > HistorySize) _totalYHistory.Dequeue();

                    double currentAverageY = _totalYHistory.Average();
                    double deltaY = currentAverageY - _lastAverageY;

                    if (Math.Abs(deltaY) >= MovementThreshold)
                    {
                        double newY = _currentYPosition + deltaY;
                        newY = Math.Max(0, newY);
                        newY = Math.Min(parent.Height - overlay.Height, newY);
                        _currentYPosition = newY;

                        var boundsY = AbsoluteLayout.GetLayoutBounds(overlay);
                        boundsY.Y = newY;
                        AbsoluteLayout.SetLayoutBounds(overlay, boundsY);

                        _lastAverageY = currentAverageY;
                    }
                    #endregion
                    break;

                case GestureStatus.Completed:
                case GestureStatus.Canceled:
                    break;
            }
        }
        #endregion

        #region Toggle Min/Max
        private bool _isCompact = false;
        private void OnToggleCompactTapped(object sender, EventArgs e)
        {
            if (CurrentState == TState.TabGeneral)
            {
                _isCompact = !_isCompact;

                if (_isCompact)
                {
                    // ascundem totul în afară de scor + FPS
                    foreach (var child in MetricsStack.Children)
                    {
                        if (child is Label lbl &&
                            //lbl != ScoreLabel &&
                            lbl != FpsLabel)
                        {
                            lbl.IsVisible = false;
                        }
                        else if (child is BoxView box)
                            box.IsVisible = false;
                    }

                    //ToggleButton.Text = "▲"; // simbol expand
                }
                else
                {
                    // afișăm tot
                    showUiItems();

                    //ToggleButton.Text = "▼"; // simbol compact
                }
            }
            else if (CurrentState == TState.TabDiagnostics)
            {
                _isCompact = !_isCompact;

                OnTabClicked(BtnTabScroll, null);
            }
        }


        #endregion


        #region Component Loading

        private void OnTabClicked(object sender, EventArgs e)
        {
            var clickedTab = (sender as Button).CommandParameter.ToString();

            if (Enum.TryParse<TState>(clickedTab, out var tab))
            {
                if (CurrentState == tab && tab != TState.TabDiagnostics)
                    return;
                else
                {
                    CurrentState = tab;
                    var boundsY = AbsoluteLayout.GetLayoutBounds(this);
                    boundsY.Y = 0;
                    boundsY.X = 0;

                    switch (tab)
                    {
                        case TState.TabGeneral:
                            MetricsStack.IsVisible = true;
                            DiagnosticsMetrics.IsVisible = false;
                            TheTreeView.IsVisible = false;
                            DiagnosticsNetwork.IsVisible = false;

                            boundsY.Width = -1;
                            boundsY.Height = -1;
                            break;
                        case TState.TabDiagnostics:
                            MetricsStack.IsVisible = false;
                            DiagnosticsMetrics.IsVisible = true;
                            TheTreeView.IsVisible = false;
                            DiagnosticsNetwork.IsVisible = false;

                            DiagnosticsMetrics.Refresh(); 
                            if (_isCompact)
                            {
                                boundsY.Width = -1;
                                boundsY.Height = -1;

                                DiagnosticsMetrics.WidthRequest = ((this.Parent as AbsoluteLayout).Width - 10) / 2;
                                DiagnosticsMetrics.HeightRequest = 200;
                            }
                            else
                            {
                                boundsY.Width = (this.Parent as AbsoluteLayout).Width;
                                boundsY.Height = (this.Parent as AbsoluteLayout).Height;

                                DiagnosticsMetrics.WidthRequest = ((this.Parent as AbsoluteLayout).Width - 10);
                                DiagnosticsMetrics.HeightRequest = ((this.Parent as AbsoluteLayout).Height - 10 - HeaderStack.Height);
                            }
                            break;
                        case TState.TabNetwork:
                            MetricsStack.IsVisible = false;
                            DiagnosticsMetrics.IsVisible = false;
                            TheTreeView.IsVisible = false;
                            DiagnosticsNetwork.IsVisible = true;

                            boundsY.Width = (this.Parent as AbsoluteLayout).Width;
                            boundsY.Height = (this.Parent as AbsoluteLayout).Height;

                            DiagnosticsNetwork.WidthRequest = boundsY.Width - 10;
                            DiagnosticsNetwork.HeightRequest = boundsY.Height - 10 - HeaderStack.Height;
                            DiagnosticsNetwork.Refresh();
                            break;
                        case TState.TabTree:
                            MetricsStack.IsVisible = false;
                            DiagnosticsMetrics.IsVisible = false;
                            TheTreeView.IsVisible = true;
                            DiagnosticsNetwork.IsVisible = false;

                            TreeNode node = _dumpService.DumpCurrentPage();
                            TheTreeView.RootNode = node;

                            boundsY.Width = (this.Parent as AbsoluteLayout).Width;
                            boundsY.Height = (this.Parent as AbsoluteLayout).Height;

                            TheTreeView.WidthRequest = boundsY.Width - 10;
                            TheTreeView.HeightRequest = boundsY.Height - 10 - HeaderStack.Height;
                            break;
                    }


                    AbsoluteLayout.SetLayoutBounds(this, boundsY);
                }
            }
        }

        #endregion

    }
}

