using MauiPerfDebugOverlay.Services;

namespace MauiPerfDebugOverlay.InternalControls
{
    public class ScrollMetricsDrawable : IDrawable
    {
        internal const float LineHeight = 40;
        private const float StartX = 10;
        private const float StartY = 40;
        private readonly Dictionary<string, RectF> _rects = new();

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            canvas.FontSize = 14;
            canvas.FontColor = Colors.White;
            canvas.Font = Microsoft.Maui.Graphics.Font.Default;

            _rects.Clear();
            float y = StartY;

            var buffer = ScrollMetricsBuffer.Instance;
            var items = GetAllMetrics();

            if (items.Count > 0)
                foreach (var kvp in items)
                {
                    string controlId = kvp.Key.ToString();
                    var metric = kvp.Value;

                    string line = $"{metric.Type} | Count: {metric.Count}, AvgDuration: {metric.AvgDuration:F2}ms, Velocity: {metric.Velocity:F1}, JankCount: {metric.JankCount}";

                    var rect = new RectF(StartX, y - LineHeight / 2, 800, LineHeight);
                    _rects[controlId] = rect;

                    // culoare în funcție de performanță
                    if (metric.AvgDuration > 50) // prag exemplu
                        canvas.FontColor = Color.FromHex("D98880");
                    else if (metric.AvgDuration > 20)
                        canvas.FontColor = Color.FromHex("FFECB3");
                    else
                        canvas.FontColor = Colors.White;

                    canvas.DrawString(line, rect, HorizontalAlignment.Left, VerticalAlignment.Top);
                    y += LineHeight;
                }
            else
            {
                string line = "No scroll metrics collected yet.";
                var rect = new RectF(StartX, y - LineHeight / 2, 800, LineHeight);
                //_rects["empty"] = rect;
                canvas.FontColor = Colors.White;
                canvas.DrawString(line, rect, HorizontalAlignment.Left, VerticalAlignment.Top);
            }
        }

        public int CountMetrics()
        {
            return GetAllMetrics().Count;
        }

        private Dictionary<Guid, ScrollMetrics> GetAllMetrics()
        {
            // Obține toate metricile existente
            var metrics = new Dictionary<Guid, ScrollMetrics>();
            // Accesăm prin reflecție internă sau putem extinde ScrollMetricsBuffer să aibă un getter pentru toate
            // Aici folosim metoda publică GetMetrics pentru simplificare, presupunem că știm Id-urile controalelor din pagina curentă
            // Dacă vrei să fie complet automat, ScrollMetricsBuffer trebuie să expună o listă a tuturor cheilor
            foreach (var controlId in ScrollMetricsBuffer.Instance.GetAllControlIds())
            {
                var metric = ScrollMetricsBuffer.Instance.GetMetrics(controlId);
                if (metric != null)
                    metrics[controlId] = metric;
            }
            return metrics;
        }
    }
}
