using MelonLoader;
using UI.Common.Options;
using UnityEngine;

[assembly: MelonInfo(typeof(FrameStats.Core), "FrameStats", "1.0.0", "oneshade", null)]
[assembly: MelonGame("CuriousOwlGames", "StellarDrive")]

namespace FrameStats {
    public class Core : MelonMod {
        private MelonPreferences_Entry<bool> _frameStatsEnabled;
        private AssetLoader _assetLoader;

        private GameObject _inGameMenu = null;
        private GameObject _activeMenuContainer = null;
        private GameObject _optionsMenu = null;
        private bool _hadOptionsMenu = false;

        public override void OnInitializeMelon() {
            MelonPreferences_Category frameStatsPreferences = MelonPreferences.CreateCategory("FrameStatsPreferences");
            _frameStatsEnabled = frameStatsPreferences.CreateEntry<bool>("FrameStatsEnabled", false);
            _assetLoader = new AssetLoader("./UserData/AssetBundles");
            /*foreach (IHardware hardware in _computer.Hardware) {
                Melon<Core>.Logger.Msg($"Hardware: {hardware.Name}");
                foreach (ISensor sensor in hardware.Sensors) {
                    Melon<Core>.Logger.Msg($"    Sensor: {sensor.Name}, type: {sensor.SensorType}, value: {sensor.Value}");
                }
            }*/
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName) {
            Melon<Core>.Logger.Msg($"scene {sceneName} loaded, index {buildIndex}");

            switch (buildIndex) {
                case 0: // MainMenu
                    _inGameMenu = null;
                    _activeMenuContainer = GameObject.Find("MainMenuCanvas/MainMenu/Menu/SelectedMenu/MenuContent");
                    break;

                case 1: // MainGame
                    _inGameMenu = GameObject.Find("UI/PauseCanvas/Canvas");
                    _activeMenuContainer = GameObject.Find("UI/PauseCanvas/Canvas/Menu");
                    break;

                default:
                    _inGameMenu = null;
                    _activeMenuContainer = null;
                    break;
            }
        }

        public override void OnFixedUpdate() {
            _optionsMenu = _activeMenuContainer?.transform.Find("OptionsMenu")?.gameObject;
            bool haveOptionsMenu = _optionsMenu != null;

            if (!_hadOptionsMenu && haveOptionsMenu) {
                Melon<Core>.Logger.Msg("have options menu");
                CreateFrameStatsSettingsUI();
            } else if (_hadOptionsMenu && !haveOptionsMenu) {
                Melon<Core>.Logger.Msg("lost options menu");
            }

            _hadOptionsMenu = haveOptionsMenu;
            if (_inGameMenu && !_inGameMenu.activeSelf) PrintStats();
        }

        private void PrintStats() {
            Melon<Core>.Logger.Msg("hello from performance monitor");
        }

        private void CreateFrameStatsSettingsUI() {
            GameObject frameStatsSettings = _assetLoader.InstantiateAsset("ui/FrameStatsSettings") as GameObject;
            if (!frameStatsSettings) {
                Melon<Core>.Logger.Error("failed to load frame stats settings ui");
                return;
            }

            frameStatsSettings.name = "FrameStats";
            frameStatsSettings.AddComponent<FrameStatsSettings>();
            frameStatsSettings.transform.SetParent(_optionsMenu.transform);
            frameStatsSettings.transform.localScale = Vector3.one;

            Melon<Core>.Logger.Msg("added stats setting option");
        }
    }
}