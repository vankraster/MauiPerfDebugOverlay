using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;
using System;
using System.Diagnostics;
using System.Timers;
using Timer = System.Timers.Timer;
namespace MauiPerfDebugOverlay.Controls
{
    public partial class PerformanceOverlayView : ContentView
    {
        private readonly Timer _updateTimer;

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
            _stopwatchGeneral = new Stopwatch();


            // Timer pentru update metrici (simulat)
            _updateTimer = new Timer(1000);
            _updateTimer.Elapsed += (s, e) =>
            {
                SimulateMetrics();
                MainThread.BeginInvokeOnMainThread(UpdateUI);
            };



        }



        private void UpdateUI()
        {
            ScoreLabel.Text = $"Overall: {_overallScore:F1}/10";
            //FpsLabel.Text = $"FPS: {_fps:F0}";
            //CpuLabel.Text = $"CPU: {_cpuUsage:F0}%";
            //MemoryLabel.Text = $"Memory: {_memoryUsage:F0} MB";
            //ThreadsLabel.Text = $"Threads: {_threadCount}"; 

            if (_overallScore >= 8)
                ScoreLabel.TextColor = Colors.LimeGreen;
            else if (_overallScore >= 5)
                ScoreLabel.TextColor = Colors.Goldenrod;
            else
                ScoreLabel.TextColor = Colors.Red;
        }

        private void SimulateMetrics()
        {
            var random = new Random();
            _overallScore = random.NextDouble() * 10;
            //_fps = 30 + random.Next(30);
            //_cpuUsage = random.Next(0, 100);
            //_memoryUsage = random.Next(100, 500);
            //_threadCount = random.Next(1, 50); 
        }




        public void Start()
        {
            startFPSCalc();


            _updateTimer.Start();

        }
        public void Stop()
        {
            //FPS   
            _stopRequested = true;


            _updateTimer.Stop();
        }



        #region FPS Calculation
        private bool _stopRequested = false;

        private int _fps;
        private Stopwatch _stopwatch;
        private Stopwatch _stopwatchGeneral;

        void startFPSCalc()
        {
            //FPS
            _fps = 0;
            _stopwatch.Restart();
            _stopwatchGeneral.Restart();

            // Rulează pe UI thread
            Application.Current!.Dispatcher.StartTimer(TimeSpan.FromMilliseconds(16), () =>
            {
                _fps++;


                //// ✅ CPU & Memory & Threads
                //var process = Process.GetCurrentProcess();
                //_memoryUsage = process.WorkingSet64 / (1024 * 1024);
                //_threadCount = process.Threads.Count;

                if (_stopwatch.ElapsedMilliseconds >= 1000)
                {

                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        // FPS
                        FpsLabel.Text = $"FPS: {_fps}";
                        if (_fps >= 50)
                            FpsLabel.TextColor = Colors.LimeGreen;
                        else if (_fps >= 30)
                            FpsLabel.TextColor = Colors.Goldenrod;
                        else
                            FpsLabel.TextColor = Colors.Red;

                        //// Memory (în MB)
                        //MemoryLabel.Text = $"Memory: {_memoryUsage} MB";
                        //if (_memoryUsage < 200)
                        //    MemoryLabel.TextColor = Colors.LimeGreen;
                        //else if (_memoryUsage < 400)
                        //    MemoryLabel.TextColor = Colors.Goldenrod;
                        //else
                        //    MemoryLabel.TextColor = Colors.Red;

                        //// Threads
                        //ThreadsLabel.Text = $"Threads: {_threadCount}";
                        //if (_threadCount < 50)
                        //    ThreadsLabel.TextColor = Colors.LimeGreen;
                        //else if (_threadCount < 100)
                        //    ThreadsLabel.TextColor = Colors.Goldenrod;
                        //else
                        //    ThreadsLabel.TextColor = Colors.Red;
                    });

                    _fps = 0;
                    _stopwatch.Restart();
                }

                return !_stopRequested; // continuă timer-ul
            });


            return;

            Application.Current!.Dispatcher.StartTimer(TimeSpan.FromMilliseconds(16), () =>
            {
                // ✅ CPU & Memory & Threads
                var process = Process.GetCurrentProcess();
                _memoryUsage = process.WorkingSet64 / (1024 * 1024);
                _threadCount = process.Threads.Count;
                var currentCpuTime = process.TotalProcessorTime;

                double cpuDelta = (currentCpuTime - _prevCpuTime).TotalMilliseconds;
                double interval = _stopwatchGeneral.Elapsed.TotalMilliseconds;

                // CPU% per proces
                _cpuUsage = (cpuDelta / interval) * 100 / _processorCount;
                // actualizează prevCpuTime
                _prevCpuTime = currentCpuTime;



                if (_stopwatchGeneral.ElapsedMilliseconds >= 1000)
                {

                    MainThread.BeginInvokeOnMainThread(() =>
                    {


                        // Memory (în MB)
                        MemoryLabel.Text = $"Memory: {_memoryUsage} MB";
                        if (_memoryUsage < 200)
                            MemoryLabel.TextColor = Colors.LimeGreen;
                        else if (_memoryUsage < 400)
                            MemoryLabel.TextColor = Colors.Goldenrod;
                        else
                            MemoryLabel.TextColor = Colors.Red;

                        // Threads
                        ThreadsLabel.Text = $"Threads: {_threadCount}";
                        if (_threadCount < 50)
                            ThreadsLabel.TextColor = Colors.LimeGreen;
                        else if (_threadCount < 100)
                            ThreadsLabel.TextColor = Colors.Goldenrod;
                        else
                            ThreadsLabel.TextColor = Colors.Red;


                        CpuLabel.Text = $"CPU: {_cpuUsage:F1}%";

                        // culoare
                        if (_cpuUsage < 30)
                            CpuLabel.TextColor = Colors.LimeGreen;
                        else if (_cpuUsage < 60)
                            CpuLabel.TextColor = Colors.Goldenrod;
                        else
                            CpuLabel.TextColor = Colors.Red;
                    });

                    _stopwatchGeneral.Restart();
                }

                return !_stopRequested; // continuă timer-ul
            });
        }
        #endregion




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
    }
}
