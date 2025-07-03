using System;
using System.ComponentModel;

namespace RawMouseThrottleBufferTester
{
    public sealed class Stats : INotifyPropertyChanged
    {
        const int Window = 512;

        readonly double[] _ring = new double[Window];
        int _head;
        int _filled;

        public double Average { get; private set; }
        public double Minimum { get; private set; }
        public double Maximum { get; private set; }
        public double StdDev { get; private set; }
        public double Current { get; private set; }
        public long Count { get; private set; }

        public void Push(double ms)
        {
            _ring[_head] = ms;
            _head = (_head + 1) % Window;
            if (_filled < Window) _filled++;

            double sum = 0, sumSq = 0;
            double min = double.MaxValue;
            double max = double.MinValue;

            for (int i = 0; i < _filled; i++)
            {
                double v = _ring[i];
                sum += v;
                sumSq += v * v;
                if (v < min) min = v;
                if (v > max) max = v;
            }

            Average = sum / _filled;
            Minimum = min;
            Maximum = max;
            StdDev = Math.Sqrt(sumSq / _filled - Average * Average);
            Current = ms;
            Count++;

            OnPropertyChanged(string.Empty);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string propName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }
}
