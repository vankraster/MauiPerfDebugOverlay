using MauiPerfDebugOverlay.Services;

namespace MauiPerfDebugOverlay.InternalControls
{
    public class DiagnosticsMetricsDrawable : IDrawable
    {
        internal const float LineHeight = 30;
        private const float StartX = 10;
        private const float StartY = 20;
        private readonly Dictionary<string, RectF> _rects = new();

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            canvas.FontSize = 14;
            canvas.Font = Microsoft.Maui.Graphics.Font.Default;

            _rects.Clear();
            float y = StartY;

            var items = DiagnosticsListener.Instance.GetAll();
            int index = 0;
            if (items.Count > 0)
            {
                foreach (var kvp in items)
                {
                    index++;

                    string metricName = kvp.Key;
                    object metricValue = kvp.Value;

                    string line = $"{metricName.Replace("dotnet.", "")} = {metricValue}";

                    var rect = new RectF(StartX, y - LineHeight / 2, dirtyRect.Width - 20, LineHeight);
                    _rects[metricName] = rect;

                    // fundal gri deschis pentru citire mai ușoară
                    if (index % 2 == 0)
                    {
                        canvas.FillColor = Color.FromArgb("#BB222222"); // un gri foarte închis
                        canvas.FillRectangle(new RectF(0, y - LineHeight / 2, dirtyRect.Width, LineHeight));
                    }

                    // culoare în funcție de tipul valorii
                    //if (metricValue is double d)
                    //{
                    //    if (d > 1000)
                    //        canvas.FontColor = Color.FromHex("D98880"); // roșu
                    //    else if (d > 100)
                    //        canvas.FontColor = Color.FromHex("FFECB3"); // galben
                    //    else
                    //        canvas.FontColor = Colors.White;
                    //}
                    //else
                    //{
                    //    canvas.FontColor = Colors.LightGreen; // fallback pt alte tipuri
                    //}



                    canvas.FontColor = Colors.White;
                    canvas.FontSize = 14;
                    canvas.DrawString(line, rect, HorizontalAlignment.Left, VerticalAlignment.Top);
                    y += LineHeight;
                }
            }
            else
            {
                string line = "No metrics collected from System.Diagnostics.Metrics yet.";
                var rect = new RectF(StartX, y - LineHeight / 2, dirtyRect.Width - 20, LineHeight);
                canvas.FontColor = Colors.White;
                canvas.DrawString(line, rect, HorizontalAlignment.Left, VerticalAlignment.Top);
            }
        }

        public int CountMetrics()
        {
            return DiagnosticsListener.Instance.GetAll().Count;
        }
    }
}
