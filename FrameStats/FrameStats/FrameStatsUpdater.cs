using MelonLoader;
using FrameStats;
using LibreHardwareMonitor.Hardware;
using UnityEngine;
using TMPro;

namespace UI.FrameStats {
    using FrameStats = global::FrameStats;

    public class FrameStatsUpdater : MonoBehaviour {
        private MelonPreferences_Entry<bool> _frameStatsEnabled;

        private TMP_Text _fieldFps;
        private TMP_Text _fieldFrameTime;

        private Computer _computer = new Computer{
            IsBatteryEnabled = true,
            IsCpuEnabled = true,
            IsGpuEnabled = true,
            IsMemoryEnabled = true
        };

        private HardwareUpdateVisitor _hardwareUpdateVisitor;
        private RollingAverager _frameRateTracker;
        private RollingAverager _frameTimeTracker;

        private void Awake() {
            MelonPreferences_Category frameStatsPreferences = MelonPreferences.GetCategory("FrameStatsPreferences");
            _frameStatsEnabled = frameStatsPreferences.GetEntry<bool>("FrameStatsEnabled");

            TMP_Text[] textComponents = GetComponentsInChildren<TMP_Text>();
            _fieldFps = textComponents[1];
            _fieldFrameTime = textComponents[3];

            _computer.Open();

            _hardwareUpdateVisitor = new HardwareUpdateVisitor();
            _frameRateTracker = new RollingAverager(240);
            _frameTimeTracker = new RollingAverager(240);

            gameObject.SetActive(_frameStatsEnabled.Value);
            _frameStatsEnabled.OnEntryValueChanged.Subscribe(OnToggleEnabled);

            Melon<FrameStats.Core>.Logger.Msg("frame stats updater awake");
        }

        private void OnDestroy() {
            _frameStatsEnabled.OnEntryValueChanged.Unsubscribe(OnToggleEnabled);
            _computer.Close();
            Melon<FrameStats.Core>.Logger.Msg("frame stats updater destroyed");
        }

        private void OnToggleEnabled(bool prevValue, bool curValue) {
            gameObject.SetActive(curValue);
        }

        private void OnEnable() {
            _frameRateTracker.Reset();
            _frameTimeTracker.Reset();
        }

        private void Update() {
            double deltaTime = Time.unscaledDeltaTime;

            double avgFrameRate = _frameRateTracker.AddSample(1.0 / deltaTime);
            _fieldFps.text = $"{avgFrameRate:0}";

            double avgFrameTime = _frameTimeTracker.AddSample(deltaTime) * 1000.0;
            _fieldFrameTime.text = $"{avgFrameTime:0.0}ms";
        }
    }
}