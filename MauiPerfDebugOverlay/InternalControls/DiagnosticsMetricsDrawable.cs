using MauiPerfDebugOverlay.Services;
using Microsoft.Maui.Controls.Shapes;

namespace MauiPerfDebugOverlay.InternalControls
{
    public class DiagnosticsMetricsDrawable : IDrawable
    {
        internal const float LineHeight = 36;
        private const float StartX = 10;
        private const float StartY = 20;
        private readonly Dictionary<string, RectF> _rects = new();

        //public void Draw(ICanvas canvas, RectF dirtyRect)
        //{
        //    canvas.FontSize = 14;
        //    canvas.Font = Microsoft.Maui.Graphics.Font.Default;

        //    _rects.Clear();
        //    float y = StartY;
        //    RectF rect = RectF.Zero;

        //    var items = DiagnosticsListener.Instance.GetAll();
        //    var itemsExceptions = DiagnosticsListener.Instance.GetAllExceptions();
        //    int index = 0;
        //    if (items.Count > 0 || itemsExceptions.Count > 0)
        //    {
        //        if (itemsExceptions.Count > 0)
        //        {
        //            rect = new RectF(StartX, y - LineHeight / 2, dirtyRect.Width - 20, LineHeight);
        //            canvas.FontColor = Colors.White;
        //            canvas.FontSize = 16;
        //            canvas.DrawString("Exceptions metrics", rect, HorizontalAlignment.Center, VerticalAlignment.Center);
        //            y += LineHeight;

        //            foreach (var kvp in itemsExceptions)
        //            {
        //                index++;

        //                string metricName = kvp.Key;
        //                object metricValue = kvp.Value;

        //                string line = $"{metricName.Replace("dotnet.", "")} = {metricValue}";

        //                rect = new RectF(StartX, y - LineHeight / 2, dirtyRect.Width - 20, LineHeight);
        //                _rects[metricName] = rect;

        //                // fundal gri deschis pentru citire mai ușoară
        //                if (index % 2 == 0)
        //                {
        //                    canvas.FillColor = Color.FromArgb("#BB222222"); // un gri foarte închis
        //                    canvas.FillRectangle(new RectF(0, y - LineHeight / 2, dirtyRect.Width, LineHeight));
        //                }

        //                canvas.FontColor = Colors.White;
        //                canvas.FontSize = 14;
        //                canvas.DrawString(line, rect, HorizontalAlignment.Left, VerticalAlignment.Center);
        //                y += LineHeight;
        //            }

        //            rect = new RectF(2, StartY - LineHeight / 2, dirtyRect.Width - 4, LineHeight);
        //            canvas.StrokeColor = Color.FromHex("D98880");
        //            canvas.DrawRectangle(rect);
        //        }




        //        rect = new RectF(2, y - LineHeight / 2, dirtyRect.Width - 4, LineHeight);
        //        canvas.StrokeColor = Color.FromHex("D98880");
        //        canvas.DrawRectangle(rect);
               
        //        canvas.FontColor = Colors.White;
        //        canvas.FontSize = 16;
        //        canvas.DrawString("Generic metrics", rect, HorizontalAlignment.Center, VerticalAlignment.Center);
        //        y += LineHeight;

        //        foreach (var kvp in items)
        //        {
        //            index++;

        //            string metricName = kvp.Key;
        //            object metricValue = kvp.Value;

        //            string line = $"{metricName.Replace("dotnet.", "")} = {metricValue}";

        //            rect = new RectF(StartX, y - LineHeight / 2, dirtyRect.Width - 20, LineHeight);
        //            _rects[metricName] = rect;

        //            // fundal gri deschis pentru citire mai ușoară
        //            if (index % 2 == 0)
        //            {
        //                canvas.FillColor = Color.FromArgb("#BB222222"); // un gri foarte închis
        //                canvas.FillRectangle(new RectF(0, y - LineHeight / 2, dirtyRect.Width, LineHeight));
        //            }
                      
        //            canvas.FontColor = Colors.White;
        //            canvas.FontSize = 14;
        //            canvas.DrawString(line, rect, HorizontalAlignment.Left, VerticalAlignment.Center);
        //            y += LineHeight;
        //        }
        //    }
        //    else
        //    {
        //        string line = "No metrics collected from System.Diagnostics.Metrics yet.";
        //        rect = new RectF(StartX, y - LineHeight / 2, dirtyRect.Width - 20, LineHeight);
        //        canvas.FontColor = Colors.White;
        //        canvas.DrawString(line, rect, HorizontalAlignment.Left, VerticalAlignment.Top);
        //    }
        //}



        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            canvas.FontSize = 14;
            canvas.Font = Microsoft.Maui.Graphics.Font.Default;

            _rects.Clear();
            float y = StartY;

            var items = DiagnosticsListener.Instance.GetAll();
            var itemsExceptions = DiagnosticsListener.Instance.GetAllExceptions();

            if (items.Count > 0 || itemsExceptions.Count > 0)
            {
                y = DrawMetricsSection(canvas, "Exceptions metrics", itemsExceptions, y, dirtyRect);
                y = DrawMetricsSection(canvas, "Generic metrics", items, y, dirtyRect);
            }
            else
            {
                string line = "No metrics collected from System.Diagnostics.Metrics yet.";
                var rect = new RectF(StartX, y - LineHeight / 2, dirtyRect.Width - 20, LineHeight);
                canvas.FontColor = Colors.White;
                canvas.DrawString(line, rect, HorizontalAlignment.Left, VerticalAlignment.Top);
            }
        }

        private float DrawMetricsSection(ICanvas canvas, string title, IReadOnlyDictionary<string, object> items, float y, RectF dirtyRect)
        {
            if (items.Count == 0)
                return y;

            // titlu
            var rect = new RectF(2, y - LineHeight / 2, dirtyRect.Width - 4, LineHeight);
            canvas.StrokeColor = Color.FromHex("D98880");
            canvas.DrawRectangle(rect);

            canvas.FontColor = Colors.White;
            canvas.FontSize = 16;
            canvas.DrawString(title, rect, HorizontalAlignment.Center, VerticalAlignment.Center);
            y += LineHeight;

            int index = 0;
            foreach (var kvp in items)
            {
                index++;
                string line = $"{kvp.Key.Replace("dotnet.", "")} = {kvp.Value}";

                rect = new RectF(StartX, y - LineHeight / 2, dirtyRect.Width - 20, LineHeight);
                _rects[kvp.Key] = rect;

                // fundal alternativ
                if (index % 2 == 0)
                {
                    canvas.FillColor = Color.FromArgb("#BB222222");
                    canvas.FillRectangle(new RectF(0, y - LineHeight / 2, dirtyRect.Width, LineHeight));
                }

                canvas.FontColor = Colors.White;
                canvas.FontSize = 14;
                canvas.DrawString(line, rect, HorizontalAlignment.Left, VerticalAlignment.Center);
                y += LineHeight;
            }

            return y;
        }


        public int CountMetrics() => DiagnosticsListener.Instance.Count();

    }
}
