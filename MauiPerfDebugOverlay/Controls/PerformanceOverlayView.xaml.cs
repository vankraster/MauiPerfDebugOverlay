
using MauiPerfDebugOverlay.Interfaces;
using MauiPerfDebugOverlay.Models;
using MauiPerfDebugOverlay.Platforms;
using System.Diagnostics;
namespace MauiPerfDebugOverlay.Controls
{
    public partial class PerformanceOverlayView : ContentView
    {
        private PerformanceOverlayOptions _options;
        private readonly IFpsService _fpsService;

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


        //overall score
        private double _emaOverallScore = 0;
        private const double _emaOverallAlpha = 0.6; // 0–1, mai mare = mai reactiv

        private double _batteryMilliW = 0;
        private bool _batteryMilliWAvailable = true;

        public PerformanceOverlayView()
        {
            InitializeComponent();

            _stopwatch = new Stopwatch();



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

            };


        }







        public void Start(PerformanceOverlayOptions options)
        {
            _options = options;
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
            if (_options.ShowBatteryUsage)
                UpdateBatteryUsage();

            if (_options.ShowNetworkStats)
                UpdateNetworkStats();

        }



        private void UpdateNetworkStats()
        {
            throw new NotImplementedException();
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
            FrameTimeLabel.Text = $"FrameTime: {_emaFrameTime:F1} ms";
            FrameTimeLabel.TextColor = _emaFrameTime <= 16 ? Colors.LimeGreen :
                                       _emaFrameTime <= 33 ? Colors.Goldenrod : Colors.Red;

            FpsLabel.Text = $"FPS: {_emaFps:F1}";
            FpsLabel.TextColor = _emaFps >= 50 ? Colors.LimeGreen :
                                 _emaFps >= 30 ? Colors.Goldenrod : Colors.Red;

            if (_emaHitch >= HitchThresholdMs)
            {
                HitchLabel.Text = $"Last Hitch EMA: {_emaHitch:F0} ms";
                HitchLabel.TextColor = Colors.Red;
            }

            AllocLabel.Text = $"Alloc/sec: {_allocPerSec:F2} MB";
            AllocLabel.TextColor = _allocPerSec < 5 ? Colors.LimeGreen :
                                   _allocPerSec < 10 ? Colors.Goldenrod : Colors.Red;

            GcLabel.Text = $"GC: Gen0 {_gc0Delta}, Gen1 {_gc1Delta}, Gen2 {_gc2Delta}";
            GcLabel.TextColor = (_gc0Delta + _gc1Delta + _gc2Delta) == 0
                ? Colors.LimeGreen : Colors.Goldenrod;

            MemoryLabel.Text = $"Memory: {_memoryUsage} MB";
            MemoryLabel.TextColor = _memoryUsage < 260 ? Colors.LimeGreen :
                                    _memoryUsage < 400 ? Colors.Goldenrod : Colors.Red;

            ThreadsLabel.Text = $"Threads: {_threadCount}";
            ThreadsLabel.TextColor = _threadCount < 50 ? Colors.LimeGreen :
                                     _threadCount < 100 ? Colors.Goldenrod : Colors.Red;

            CpuLabel.Text = $"CPU: {_cpuUsage:F1}%";
            CpuLabel.TextColor = _cpuUsage < 30 ? Colors.LimeGreen :
                                 _cpuUsage < 60 ? Colors.Goldenrod : Colors.Red;

            ScoreLabel.Text = $"Overall: {_emaOverallScore:F1}/10";
            ScoreLabel.TextColor = _emaOverallScore >= 8 ? Colors.LimeGreen :
                                   _emaOverallScore >= 5 ? Colors.Goldenrod : Colors.Red;


            if (_batteryMilliWAvailable)
            {
                BatteryLabel.Text = $"Battery consumption: {_batteryMilliW:F1} mW";
                BatteryLabel.TextColor = _batteryMilliW < 100 ? Colors.LimeGreen :
                                         _batteryMilliW < 500 ? Colors.Goldenrod : Colors.Red;
            }
            else
            {
                BatteryLabel.Text = "Battery consumption: N/A";
                BatteryLabel.TextColor = Colors.Gray;
            }

        }


        private void StartMetrics()
        {
            _stopwatch.Restart();
            _prevCpuTime = _currentProcess.TotalProcessorTime;

            Application.Current!.Dispatcher.StartTimer(TimeSpan.FromSeconds(1), () =>
            {
                UpdateGcAndAllocMetrics();

                UpdateExtraMetrics();

                _memoryUsage = _currentProcess.WorkingSet64 / (1024 * 1024);
                _threadCount = _currentProcess.Threads.Count;

                var currentCpuTime = _currentProcess.TotalProcessorTime;
                double cpuDelta = (currentCpuTime - _prevCpuTime).TotalMilliseconds;

                double interval = _stopwatch.Elapsed.TotalMilliseconds; // ⬅️ real interval
                _cpuUsage = (cpuDelta / interval) * 100 / _processorCount;

                _prevCpuTime = currentCpuTime;
                _stopwatch.Restart();

                _overallScore = CalculateOverallScore();
                UpdateOverallScore(_overallScore);

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
            _isCompact = !_isCompact;

            if (_isCompact)
            {
                // ascundem totul în afară de scor + FPS
                foreach (var child in MetricsStack.Children)
                {
                    if (child is Label lbl &&
                        lbl != ScoreLabel &&
                        lbl != FpsLabel)
                    {
                        lbl.IsVisible = false;
                    }

                }

                //ToggleButton.Text = "▲"; // simbol expand
            }
            else
            {
                // afișăm tot
                foreach (var child in MetricsStack.Children)
                {
                    (child as View).IsVisible = true;
                }

                //ToggleButton.Text = "▼"; // simbol compact
            }
        }
    }
    #endregion
}

