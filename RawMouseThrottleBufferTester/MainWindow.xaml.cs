using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

using Linearstar.Windows.RawInput;

namespace RawMouseThrottleBufferTester
{
    public partial class MainWindow : Window
    {
        const int WM_INPUT = 0x00FF;

        readonly Stats _stats = new Stats();
        readonly double _msPerTick = 1000.0 / Stopwatch.Frequency;
        long _prevTicks;

        const double MaxGapMs = 40.0;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = _stats;

            CompositionTarget.Rendering += (_, __) =>
                Title = $"RawMouseThrottle - avg {_stats.Average:F1} ms";
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            var src = (HwndSource)PresentationSource.FromVisual(this);
            src.AddHook(WndProc);

            RawInputDevice.RegisterDevice(
                HidUsageAndPage.Mouse,
                RawInputDeviceFlags.InputSink,
                src.Handle);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam,
                               IntPtr lParam, ref bool handled)
        {
            if (msg == WM_INPUT &&
                RawInputData.FromHandle(lParam) is RawInputMouseData mouse)
            {
                // интересуют только «движения»
                if (mouse.Mouse.LastX == 0 && mouse.Mouse.LastY == 0)
                    return IntPtr.Zero;

                long now = Stopwatch.GetTimestamp();
                if (_prevTicks != 0)
                {
                    double dt = (now - _prevTicks) * _msPerTick;
                    if (dt < MaxGapMs) _stats.Push(dt);
                }
                _prevTicks = now;
            }
            return IntPtr.Zero;
        }
    }
}
