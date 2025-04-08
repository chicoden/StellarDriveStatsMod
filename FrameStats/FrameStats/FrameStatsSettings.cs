using MelonLoader;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Common.Options {
    using FrameStats = global::FrameStats;

    public class FrameStatsSettings : MonoBehaviour {
        private MelonPreferences_Entry<bool> _frameStatsEnabled;

        private void Awake() {
            MelonPreferences_Category frameStatsPreferences = MelonPreferences.GetCategory("FrameStatsPreferences");
            _frameStatsEnabled = frameStatsPreferences.GetEntry<bool>("FrameStatsEnabled");

            Toggle toggle = GetComponentInChildren<Toggle>();
            toggle.isOn = _frameStatsEnabled.Value;
            toggle.onValueChanged.AddListener(OnToggleSwitch);
        }

        private void OnToggleSwitch(bool value) {
            _frameStatsEnabled.Value = value;
            Melon<FrameStats.Core>.Logger.Msg($"Toggle: {value}");
        }
    }
}