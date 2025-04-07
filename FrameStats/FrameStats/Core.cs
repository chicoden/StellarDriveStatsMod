using MelonLoader;
using LibreHardwareMonitor.Hardware;
using UnityEngine;
using UI.Common.Options;

[assembly: MelonInfo(typeof(FrameStats.Core), "FrameStats", "1.0.0", "oneshade", null)]
[assembly: MelonGame("CuriousOwlGames", "StellarDrive")]

namespace FrameStats {
    public class Core : MelonMod {
        private AssetLoader _assetLoader;

        private GameObject _inGameMenu = null;
        private GameObject _activeMenuContainer = null;
        private GameObject _optionsMenu = null;
        private bool _hadOptionsMenu = false;

        private static Computer _computer = new Computer{
            IsBatteryEnabled = true,
            IsCpuEnabled = true,
            IsGpuEnabled = true,
            IsMemoryEnabled = true
        };

        public override void OnLateInitializeMelon() {
            _assetLoader = new AssetLoader(Melon<Core>.Logger, "./UserData/AssetBundles");

            /******************************************************************************/
            _computer.Open();
            _computer.Accept(new HardwareUpdateVisitor());

            foreach (IHardware hardware in _computer.Hardware) {
                Melon<Core>.Logger.Msg($"Hardware: {hardware.Name}");
                foreach (IHardware subhardware in hardware.SubHardware) {
                    Melon<Core>.Logger.Msg($"    Subhardware: {subhardware.Name}");
                    foreach (ISensor sensor in subhardware.Sensors) {
                        Melon<Core>.Logger.Msg($"        Sensor: {sensor.Name}, value: {sensor.Value}");
                    }
                }

                foreach (ISensor sensor in hardware.Sensors) {
                    Melon<Core>.Logger.Msg($"    Sensor: {sensor.Name}, value: {sensor.Value}");
                }
            }

            _computer.Close();
            /******************************************************************************/
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
            GameObject frameStatsSettings = _assetLoader.GetAsset("ui/FrameStatsSettings") as GameObject;
            if (!frameStatsSettings) {
                Melon<Core>.Logger.Error("failed to load frame stats settings ui");
                return;
            }

            frameStatsSettings.AddComponent<FrameStatsSettings>();
            frameStatsSettings.transform.SetParent(_optionsMenu.transform);
            Melon<Core>.Logger.Msg("added stats setting option");
        }
    }
}