namespace FrameStats {
    public class RollingAverager {
        private double[] _samples;
        private double _runningTotal;
        private double _runningAverage;
        private int _activeSampleCount;
        private int _queueHeadIndex;

        public RollingAverager(int maxSamples) {
            _samples = new double[maxSamples];
            _runningTotal = 0;
            _runningAverage = 0;
            _activeSampleCount = 0;
            _queueHeadIndex = 0;
        }

        public double AddSample(double sample) {
            if (_activeSampleCount == _samples.Length) {
                _runningTotal -= _samples[_queueHeadIndex];
            }

            _runningTotal += sample;
            _samples[_queueHeadIndex] = sample;
            _queueHeadIndex = (_queueHeadIndex + 1) % _samples.Length;

            if (_activeSampleCount < _samples.Length) {
                _activeSampleCount++;
            }

            _runningAverage = _runningTotal / _activeSampleCount;
            return _runningAverage;
        }

        public void Reset() {
            _runningTotal = 0;
            _runningAverage = 0;
            _activeSampleCount = 0;
            _queueHeadIndex = 0;
        }
    }
}