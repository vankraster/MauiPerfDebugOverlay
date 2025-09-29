using MauiPerfDebugOverlay.Controls;
using Microsoft.Maui.Layouts;
using System.Reflection;

namespace MauiPerfDebugOverlay.Services
{
    public class PerformanceOverlayManager
    {
        private static PerformanceOverlayManager? _instance;
        private PerformanceOverlayView? _overlay;

        public static PerformanceOverlayManager Instance =>
            _instance ??= new PerformanceOverlayManager();

        private PerformanceOverlayManager() { }

        /// <summary>
        /// Activează overlay-ul global.
        /// Trebuie apelat o singură dată în MauiProgram.cs
        /// </summary>
        public void Enable()
        {
            Application.Current.PageAppearing += OnPageAppearing;

            #region CheckLatestVersion
            //https://api.nuget.org/v3-flatcontainer/PerformanceDebugOverlay/index.json


            Task.Run(async () =>
            {
                int majorLine = 1; // sau 2, depinde de linia ta de .NET 
                string currentVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.0.0";

                var latestVersion = VersionChecker.GetLatestNugetVersionForMajor(majorLine);

                if (VersionChecker.IsNewerVersionAvailable(currentVersion, latestVersion))
                    VersionChecker.NewVersionLabelText = $"New version {latestVersion} available on NuGet!";

            });
            #endregion
        }

        private void OnPageAppearing(object? sender, Page page)
        {
            if (page is not ContentPage contentPage || contentPage.Content == null)
                return;

            // Dacă contentul nu e AbsoluteLayout, împachetează-l
            AbsoluteLayout abs;
            if (contentPage.Content is AbsoluteLayout existingLayout)
            {
                abs = existingLayout;
            }
            else
            {
                abs = new AbsoluteLayout();

                var originalContent = contentPage.Content;
                contentPage.Content = null; // eliminăm temporar
                AbsoluteLayout.SetLayoutFlags(originalContent, AbsoluteLayoutFlags.All);
                AbsoluteLayout.SetLayoutBounds(originalContent, new Rect(0, 0, 1, 1));
                abs.Children.Add(originalContent);

                contentPage.Content = abs;
            }

            // Verifică dacă overlay-ul există deja pe pagină
            if (!abs.Children.OfType<PerformanceOverlayView>().Any())
            {
                var overlay = new PerformanceOverlayView();
                AbsoluteLayout.SetLayoutFlags(overlay, AbsoluteLayoutFlags.None);
                AbsoluteLayout.SetLayoutBounds(overlay, new Rect(0, 0, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));
                abs.Children.Add(overlay);
                overlay.Start();
            }
        }






        public event Action<bool>? VisibilityChanged;
        public static bool LastVisibilityState = true;
        public void Hide()
        {
            LastVisibilityState = false;
            VisibilityChanged?.Invoke(false);
        }

        public void Show()
        {
            LastVisibilityState = true;
            VisibilityChanged?.Invoke(true);
        }
    }
}
