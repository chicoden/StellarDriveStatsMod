using MelonLoader;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Common.Options {
    public class FrameStatsSettings : MonoBehaviour {
        private MelonPreferences_Entry<bool> _frameStatsEnabled;

        private void Start() {
            MelonPreferences_Category frameStatsPreferences = MelonPreferences.GetCategory("FrameStatsPreferences");
            _frameStatsEnabled = frameStatsPreferences.GetEntry<bool>("FrameStatsEnabled");

            Toggle toggle = gameObject.transform.GetChild(1).gameObject.GetComponent<Toggle>();
            toggle.isOn = _frameStatsEnabled.Value;
            toggle.onValueChanged.AddListener(OnToggleSwitch);
        }

        public void OnToggleSwitch(bool value) {
            _frameStatsEnabled.Value = value;
            Melon<FrameStats.Core>.Logger.Msg($"Toggle: {value}");
        }
    }
}