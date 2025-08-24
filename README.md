# MauiPerfDebugOverlay
A lightweight, drag-and-move performance HUD for .NET MAUI apps, showing FPS, memory, CPU, threads, frame drops, and an overall score.

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


<img width="434" height="734" alt="image" src="https://github.com/user-attachments/assets/1ed95dfa-d4ba-49cc-9ac1-37a7f6b3c0b0" />
