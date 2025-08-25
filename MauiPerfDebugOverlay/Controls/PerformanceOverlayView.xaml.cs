using MauiPerfDebugOverlay.Interfaces;
using System.Diagnostics;
namespace MauiPerfDebugOverlay.Controls
{
    public partial class PerformanceOverlayView : ContentView
    {
        private readonly IFpsService _fpsService;

        private double _overallScore;
        private double _cpuUsage;
        private TimeSpan _prevCpuTime;
        private double _memoryUsage;
        private int _threadCount;
        private int _processorCount = Environment.ProcessorCount;

        public PerformanceOverlayView()
        {
            InitializeComponent();

            _stopwatch = new Stopwatch();



#if ANDROID || IOS || MACCATALYST || WINDOWS || TIZEN
            _fpsService = new FpsService();
            _fpsService.OnFrameTimeCalculated += frameTimeMs =>
            {
                _frameTime = frameTimeMs;
                _fps = (int)(1000.0 / frameTimeMs);

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    FrameTimeLabel.Text = $"FrameTime: {_frameTime:F1} ms";
                    FrameTimeLabel.TextColor = _frameTime <= 16 ? Colors.LimeGreen :
                                                      _frameTime <= 33 ? Colors.Goldenrod : Colors.Red;

                    FpsLabel.Text = $"FPS: {_fps}";
                    FpsLabel.TextColor = _fps >= 50 ? Colors.LimeGreen :
                                            _fps >= 30 ? Colors.Goldenrod : Colors.Red;
                });
            };
#endif
        }








        public void Start()
        {
            _fpsService?.Start();
            StartMetrics();
        }
        public void Stop()
        {
            _fpsService?.Stop();
            _stopRequested = true;
        }



        private bool _stopRequested = false;

        private int _fps;
        private double _frameTime;
        private Stopwatch _stopwatch;




        private void StartMetrics()
        {
            _fps = 0;
            _stopwatch.Restart();
            _prevCpuTime = Process.GetCurrentProcess().TotalProcessorTime;

            Application.Current!.Dispatcher.StartTimer(TimeSpan.FromMilliseconds(16), () =>
            {
                _fps++;

                // freeze detect: dacă UI-ul a stat blocat prea mult
                if (_stopwatch.ElapsedMilliseconds > 2000)
                {
                    _stopwatch.Restart();
                    return !_stopRequested;
                }

                if (_stopwatch.ElapsedMilliseconds >= 1000)
                {
                    var process = Process.GetCurrentProcess();
                    _memoryUsage = process.WorkingSet64 / (1024 * 1024);
                    _threadCount = process.Threads.Count;

                    var currentCpuTime = process.TotalProcessorTime;
                    double cpuDelta = (currentCpuTime - _prevCpuTime).TotalMilliseconds;
                    double interval = _stopwatch.Elapsed.TotalMilliseconds;
                    _cpuUsage = (cpuDelta / interval) * 100 / _processorCount;
                    _prevCpuTime = currentCpuTime;


                    _overallScore = CalculateOverallScore();


                    MainThread.BeginInvokeOnMainThread(() =>
                    {


                        // Memory
                        MemoryLabel.Text = $"Memory: {_memoryUsage} MB";
                        MemoryLabel.TextColor = _memoryUsage < 260 ? Colors.LimeGreen :
                                                _memoryUsage < 400 ? Colors.Goldenrod : Colors.Red;

                        // Threads
                        ThreadsLabel.Text = $"Threads: {_threadCount}";
                        ThreadsLabel.TextColor = _threadCount < 50 ? Colors.LimeGreen :
                                                 _threadCount < 100 ? Colors.Goldenrod : Colors.Red;

                        // CPU
                        CpuLabel.Text = $"CPU: {_cpuUsage:F1}%";
                        CpuLabel.TextColor = _cpuUsage < 30 ? Colors.LimeGreen :
                                             _cpuUsage < 60 ? Colors.Goldenrod : Colors.Red;

                        // Overall
                        ScoreLabel.Text = $"Overall: {_overallScore:F1}/10";
                        ScoreLabel.TextColor = _overallScore >= 8 ? Colors.LimeGreen :
                                               _overallScore >= 5 ? Colors.Goldenrod : Colors.Red;
                    });

                    _fps = 0;
                    _stopwatch.Restart();
                }

                return !_stopRequested;
            });
        }


        private double CalculateOverallScore()
        {
            double score = 0;

            // FPS (max 3 puncte)
            if (_fps >= 50) score += 3;
            else if (_fps >= 30) score += 2;
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




        #region Drag && Move

        private const int HistorySize = 5;
        private const double MovementThreshold = 1.0;

        private readonly Queue<double> _totalXHistory = new Queue<double>();
        private readonly Queue<double> _totalYHistory = new Queue<double>();
        private double _lastAverageX;
        private double _lastAverageY;
        private double _currentXPosition;
        private double _currentYPosition;

        private void OnPanUpdated(object? sender, PanUpdatedEventArgs e)
        {
            if (sender is not View overlay) return;
            if (overlay.Parent.Parent is not Grid parent) return;

            switch (e.StatusType)
            {
                case GestureStatus.Started:
                    _currentXPosition = TranslationX;
                    _totalXHistory.Clear();
                    _totalXHistory.Enqueue(e.TotalX);
                    _lastAverageX = e.TotalX;

                    _currentYPosition = TranslationY;
                    _totalYHistory.Clear();
                    _totalYHistory.Enqueue(e.TotalY);
                    _lastAverageY = e.TotalY;
                    break;

                case GestureStatus.Running:
                    #region X Axis
                    _totalXHistory.Enqueue(e.TotalX);
                    if (_totalXHistory.Count > HistorySize)
                        _totalXHistory.Dequeue();

                    double currentAverageX = _totalXHistory.Average();
                    double deltaX = currentAverageX - _lastAverageX;

                    if (Math.Abs(deltaX) >= MovementThreshold)
                    {
                        double newX = _currentXPosition + deltaX;

                        // Limitează la marginea ecranului
                        newX = Math.Max(0, newX);
                        newX = Math.Min(parent.Width - overlay.Width, newX);

                        _currentXPosition = newX;

                        TranslationX = newX;

                        _lastAverageX = currentAverageX;
                    }
                    #endregion

                    #region Y Axis
                    _totalYHistory.Enqueue(e.TotalY);
                    if (_totalYHistory.Count > HistorySize)
                        _totalYHistory.Dequeue();

                    double currentAverageY = _totalYHistory.Average();
                    double deltaY = currentAverageY - _lastAverageY;

                    if (Math.Abs(deltaY) >= MovementThreshold)
                    {
                        double newY = _currentYPosition + deltaY;

                        // Limitează între 0% și 100% din înălțimea ecranului
                        newY = Math.Max(parent.Height * 0.00, newY);
                        newY = Math.Min(parent.Height * 1 - overlay.Height, newY);

                        _currentYPosition = newY;

                        TranslationY = newY;
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

