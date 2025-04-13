namespace FrameStats {
    public struct Sample {
        public readonly double CumulativeValue;
        public readonly TimeSpan Interval;

        public Sample(double value, TimeSpan interval) {
            CumulativeValue = value * interval.TotalSeconds;
            Interval = interval;
        }
    }

    public class RollingAverager {
        private Queue<Sample> _samples;
        private TimeSpan _totalInterval;
        private TimeSpan _maxInterval;
        private double _runningTotal;

        public RollingAverager(TimeSpan window) {
            _samples = new Queue<Sample>();
            _totalInterval = TimeSpan.Zero;
            _maxInterval = window;
            _runningTotal = 0;
        }

        public double AddSample(Sample sample) {
            _samples.Enqueue(sample);
            _totalInterval += sample.Interval;
            _runningTotal += sample.CumulativeValue;

            while (_totalInterval > _maxInterval && _samples.Count > 1) {
                Sample outdatedSample = _samples.Dequeue();
                _totalInterval -= outdatedSample.Interval;
                _runningTotal -= outdatedSample.CumulativeValue;
            }

            return _runningTotal / _totalInterval.TotalSeconds;
        }

        public void Reset() {
            _samples.Clear();
            _totalInterval = TimeSpan.Zero;
            _runningTotal = 0;
        }
    }
}