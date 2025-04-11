using MelonLoader;
using FrameStats;
using UnityEngine;
using TMPro;
using Lock = System.Object;

namespace UI.FrameStats {
    using FrameStats = global::FrameStats;

    public class FrameStatsUpdater : MonoBehaviour {
        private MelonPreferences_Entry<bool> _frameStatsEnabled;

        private TMP_Text _fieldFps;
        private TMP_Text _fieldFrameTime;

        private RollingAverager _frameRateTracker;
        private RollingAverager _frameTimeTracker;

        private Thread _hwMonitorThread;
        private bool _hwMonitorSentinel;
        private Lock _hwMonitorSentinelLock;

        private void Awake() {
            MelonPreferences_Category frameStatsPreferences = MelonPreferences.GetCategory("FrameStatsPreferences");
            _frameStatsEnabled = frameStatsPreferences.GetEntry<bool>("FrameStatsEnabled");

            TMP_Text[] textComponents = GetComponentsInChildren<TMP_Text>();
            _fieldFps = textComponents[1];
            _fieldFrameTime = textComponents[3];

            _frameRateTracker = new RollingAverager(240);
            _frameTimeTracker = new RollingAverager(240);

            _hwMonitorThread = null;
            _hwMonitorSentinelLock = new Lock();

            gameObject.SetActive(_frameStatsEnabled.Value);
            _frameStatsEnabled.OnEntryValueChanged.Subscribe(OnToggleEnabled);
        }

        private void OnDestroy() {
            _frameStatsEnabled.OnEntryValueChanged.Unsubscribe(OnToggleEnabled);
            _hwMonitorThread?.Join();
        }

        private void OnEnable() {
            _frameRateTracker.Reset();
            _frameTimeTracker.Reset();

            _hwMonitorThread?.Join();
            _hwMonitorSentinel = true;
            ThreadStart hwMonitorDelegate = new ThreadStart(MonitorHardware);
            _hwMonitorThread = new Thread(hwMonitorDelegate);
            _hwMonitorThread.Start();
        }

        private void OnDisable() {
            lock (_hwMonitorSentinelLock) {
                _hwMonitorSentinel = false;
            }

            Melon<FrameStats.Core>.Logger.Msg("waiting for hw monitor thread to finish...");
        }

        private void Update() {
            double deltaTime = Time.unscaledDeltaTime;

            double avgFrameRate = _frameRateTracker.AddSample(1.0 / deltaTime);
            _fieldFps.text = $"{avgFrameRate:0}";

            double avgFrameTime = _frameTimeTracker.AddSample(deltaTime) * 1000.0;
            _fieldFrameTime.text = $"{avgFrameTime:0.0}ms";
        }

        private void OnToggleEnabled(bool prevValue, bool curValue) {
            gameObject.SetActive(curValue);
        }

        private void MonitorHardware() {
            Melon<FrameStats.Core>.Logger.Msg("hw monitor thread started");

            bool running = true;
            while (running) {
                Melon<FrameStats.Core>.Logger.Msg("hello from hw monitor thread");
                Thread.Sleep(1000);
                lock (_hwMonitorSentinelLock) {
                    running = _hwMonitorSentinel;
                }
            }

            Melon<FrameStats.Core>.Logger.Msg("hw monitor thread finished");
        }
    }
}