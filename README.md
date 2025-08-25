# MauiPerfDebugOverlay

A lightweight, drag-and-move performance HUD for .NET MAUI apps, showing real-time metrics including:

- FPS and Frame Time (ms) with EMA smoothing  
- CPU usage (%)  
- Memory usage (MB)  
- Threads count  
- GC Collections (Gen0/Gen1/Gen2)  
- Allocations per second (MB/sec)  
- UI thread responsiveness (hitches)  
- Overall performance score (0–10)  
- Overlay mode toggle (compact / expanded)  
- Fully draggable overlay that works across pages  

Designed for quick integration in .NET MAUI apps with minimal performance overhead.

How To Use
```csharp 
 public App()
 {
     InitializeComponent();

     // Activate PerformanceOverlay global
     PerformanceOverlayManager.Instance.Enable();

     MainPage = new AppShell();
 }
```

<img width="431" height="795" alt="image" src="https://github.com/user-attachments/assets/c930cdc8-9abf-49e0-9abd-d88f8e070058" />

