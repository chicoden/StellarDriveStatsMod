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
        private TMP_Text _fieldBatteryLevel;

        private RollingAverager _frameRateTracker;
        private RollingAverager _frameTimeTracker;

        private Lock _hwStatsLock;
        private string _dummyStat;

        private volatile bool _flagKeepHwMonitorAlive;
        private volatile bool _flagKeepHwMonitorMonitoring;
        private Thread _hwMonitorThread;

        private void Awake() {
            MelonPreferences_Category frameStatsPreferences = MelonPreferences.GetCategory("FrameStatsPreferences");
            _frameStatsEnabled = frameStatsPreferences.GetEntry<bool>("FrameStatsEnabled");

            TMP_Text[] textComponents = GetComponentsInChildren<TMP_Text>();
            _fieldFps = textComponents[1];
            _fieldFrameTime = textComponents[3];
            _fieldBatteryLevel = textComponents[5];

            _frameRateTracker = new RollingAverager(240);
            _frameTimeTracker = new RollingAverager(240);

            _hwStatsLock = new Lock();
            _dummyStat = "10.3%";

            _flagKeepHwMonitorAlive = false;
            _flagKeepHwMonitorMonitoring = false;
            ThreadStart hwMonitorDelegate = new ThreadStart(MonitorHardware);
            _hwMonitorThread = new Thread(hwMonitorDelegate);

            gameObject.SetActive(_frameStatsEnabled.Value);
            _frameStatsEnabled.OnEntryValueChanged.Subscribe(OnToggleEnabled);
        }

        private void OnDestroy() {
            _frameStatsEnabled.OnEntryValueChanged.Unsubscribe(OnToggleEnabled);
            if (_hwMonitorThread.ThreadState != ThreadState.Unstarted) {
                _flagKeepHwMonitorAlive = false;
                _hwMonitorThread.Join();
            }
        }

        private void OnEnable() {
            _frameRateTracker.Reset();
            _frameTimeTracker.Reset();

            _flagKeepHwMonitorMonitoring = true;
            if (_hwMonitorThread.ThreadState == ThreadState.Unstarted) {
                _flagKeepHwMonitorAlive = true;
                _hwMonitorThread.Start();
            }
        }

        private void OnDisable() {
            _flagKeepHwMonitorMonitoring = false;
        }

        private void Update() {
            double deltaTime = Time.unscaledDeltaTime;

            double avgFrameRate = _frameRateTracker.AddSample(1.0 / deltaTime);
            _fieldFps.text = $"{avgFrameRate:0}";

            double avgFrameTime = _frameTimeTracker.AddSample(deltaTime) * 1000.0;
            _fieldFrameTime.text = $"{avgFrameTime:0.0}ms";

            lock (_hwStatsLock) {
                _fieldBatteryLevel.text = _dummyStat;
            }
        }

        private void OnToggleEnabled(bool prevValue, bool curValue) {
            gameObject.SetActive(curValue);
        }

        private void MonitorHardware() {
            Melon<FrameStats.Core>.Logger.Msg("entering hw monitor");

            while (_flagKeepHwMonitorAlive) {
                double newDummyValue = 0.0;
                while (_flagKeepHwMonitorMonitoring) {
                    newDummyValue += 0.01;
                    string newDummyStat = $"{newDummyValue:0.0}%";

                    lock (_hwStatsLock) {
                        _dummyStat = newDummyStat;
                    }

                    Thread.Sleep(50);
                }

                Thread.Sleep(50);
            }

            Melon<FrameStats.Core>.Logger.Msg("exiting hw monitor");
        }
    }
}