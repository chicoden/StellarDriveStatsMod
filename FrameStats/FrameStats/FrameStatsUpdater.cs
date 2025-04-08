using MelonLoader;
using FrameStats;
using LibreHardwareMonitor.Hardware;
using UnityEngine;

namespace UI.FrameStats {
    using FrameStats = global::FrameStats;

    public class FrameStatsUpdater : MonoBehaviour {
        private MelonPreferences_Entry<bool> _frameStatsEnabled;

        private Computer _computer = new Computer{
            IsBatteryEnabled = true,
            IsCpuEnabled = true,
            IsGpuEnabled = true,
            IsMemoryEnabled = true
        };

        private HardwareUpdateVisitor _hardwareUpdateVisitor;

        private void Awake() {
            MelonPreferences_Category frameStatsPreferences = MelonPreferences.GetCategory("FrameStatsPreferences");
            _frameStatsEnabled = frameStatsPreferences.GetEntry<bool>("FrameStatsEnabled");

            _computer.Open();
            _hardwareUpdateVisitor = new HardwareUpdateVisitor();

            gameObject.SetActive(_frameStatsEnabled.Value);
            _frameStatsEnabled.OnEntryValueChanged.Subscribe(OnToggleEnabled);

            Melon<FrameStats.Core>.Logger.Msg("frame stats updater awake");
        }

        private void OnDestroy() {
            _frameStatsEnabled.OnEntryValueChanged.Unsubscribe(OnToggleEnabled);
            _computer.Close();
            Melon<FrameStats.Core>.Logger.Msg("frame stats updater destroyed");
        }

        private void Update() {
            Melon<FrameStats.Core>.Logger.Msg("frame stats update");
            /*foreach (IHardware hardware in _computer.Hardware) {
                Melon<FrameStats.Core>.Logger.Msg($"Hardware: {hardware.Name}");
                foreach (ISensor sensor in hardware.Sensors) {
                    Melon<FrameStats.Core>.Logger.Msg($"    Sensor: {sensor.Name}, type: {sensor.SensorType}, value: {sensor.Value}");
                }
            }*/
        }

        private void OnToggleEnabled(bool prevValue, bool curValue) {
            gameObject.SetActive(curValue);
        }
    }
}