\## PerformanceDebugOverlay (v2.0.3)
---
  
A real-time performance overlay for .NET MAUI that tracks FPS, CPU, memory, GC, battery, and network usage, while providing a load-time component tree—all without major changes to your app code.



![Overlay Screenshot](https://raw.githubusercontent.com/vankraster/MauiPerfDebugOverlay/refs/heads/master/MauiPerfDebugOverlay.SampleApp/overlay/overlay-screenshot-203.png)


\## Features
---


FPS \& FrameTime – calculated using EMA (Exponential Moving Average)

CPU usage – per-process CPU utilization

Memory usage – current memory and allocations/sec

GC activity – collections per generation

Battery consumption – approximate consumption in mW (Android only)

Network stats – total requests, bytes sent/received, average request time + AI

Compact / Expanded view – show/hide individual metrics

Live drag \& reposition – move the overlay freely at runtime

Plug-and-play integration – works globally without modifying existing HTTP code

Tree View of Load-Time Components with Metrics + AI

Collapse/Expand tree view items  

Scrollable TreeView

AI assistance for performance improvement suggestions ( use your own Gemini API key )

\## Installation
---

Add the NuGet package to your project:



dotnet add package PerformanceDebugOverlay --version 2.0.3



---



\## Configuration 



in MauiProgram.cs



```bash



using MauiPerfDebugOverlay.Extensions;
using MauiPerfDebugOverlay.Models;

public static class MauiProgram
{

  public static MauiApp CreateMauiApp()
  {
     var builder = MauiApp.CreateBuilder();

     builder
         .UseMauiApp<App>()
         .UsePerformanceDebugOverlay(new PerformanceOverlayOptions

         {
             ShowBatteryUsage = true,
             ShowNetworkStats = true,
             ShowAlloc_GC = true,
             ShowCPU_Usage = true,
             ShowFrame = true,
             ShowMemory = true,
             ShowLoadTime = true,
             LoadTimeDanger = 200,
             LoadTimeWarning = 450
             GeminiAPIKey = "YOUR_API_KEY" // optional
         });
     return builder.Build();
  }
}

```



Options are optional and can be enabled or disabled individually.

Activation in Your App

In App.xaml.cs:

```bash

public App()
{
 InitializeComponent();

 // Enable the PerformanceOverlay globally

 PerformanceOverlayManager.Instance.Enable();
 MainPage = new AppShell();
}

```

The overlay will appear automatically and is interactive.


\## Notes
---

Battery consumption metrics are only available on Android. Other platforms will display N/A.



Networking metrics automatically monitor all HttpClient and HttpWebRequest requests without modifying existing code.


Overlay is fully configurable and extensible.


\## Simple API
---

```bash

PerformanceOverlayManager.Instance.Enable() – show the overlay



PerformanceOverlayManager.Instance.Disable() – hide the overlay

```


Configuration is done via PerformanceOverlayOptions to customize which metrics are displayed
