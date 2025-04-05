using MelonLoader;
using LibreHardwareMonitor.Hardware;
using UnityEngine;

[assembly: MelonInfo(typeof(FrameStats.Core), "FrameStats", "1.0.0", "oneshade", null)]
[assembly: MelonGame("CuriousOwlGames", "StellarDrive")]

namespace FrameStats {
    public class Core : MelonMod {
        private GameObject inGameMenu = null;
        private GameObject activeMenuContainer = null;
        private GameObject optionsMenu = null;
        private bool hadOptionsMenu = false;

        private static Computer computer = new Computer{
            IsBatteryEnabled = true,
            IsCpuEnabled = true,
            IsGpuEnabled = true,
            IsMemoryEnabled = true
        };

        public override void OnInitializeMelon() {
            Melon<Core>.Logger.Msg("melon initialized");

            computer.Open();
            computer.Accept(new HardwareUpdateVisitor());

            foreach (IHardware hardware in computer.Hardware) {
                Melon<Core>.Logger.Msg("Hardware: {0}", hardware.Name);
                foreach (IHardware subhardware in hardware.SubHardware) {
                    Melon<Core>.Logger.Msg("\tSubhardware: {0}", subhardware.Name);
                    foreach (ISensor sensor in subhardware.Sensors) {
                        Melon<Core>.Logger.Msg("\t\tSensor: {0}, value: {1}", sensor.Name, sensor.Value);
                    }
                }

                foreach (ISensor sensor in hardware.Sensors) {
                    Melon<Core>.Logger.Msg("\tSensor: {0}, value: {1}", sensor.Name, sensor.Value);
                }
            }

            computer.Close();
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName) {
            Melon<Core>.Logger.Msg($"scene {sceneName} loaded, index {buildIndex}");

            switch (buildIndex) {
                case 0: // MainMenu
                    inGameMenu = null;
                    activeMenuContainer = GameObject.Find("MainMenuCanvas/MainMenu/Menu/SelectedMenu/MenuContent");
                    break;

                case 1: // MainGame
                    inGameMenu = GameObject.Find("UI/PauseCanvas/Canvas");
                    activeMenuContainer = GameObject.Find("UI/PauseCanvas/Canvas/Menu");
                    break;

                default:
                    inGameMenu = null;
                    activeMenuContainer = null;
                    break;
            }
        }

        public override void OnFixedUpdate() {
            optionsMenu = activeMenuContainer?.transform.Find("OptionsMenu")?.gameObject;
            bool haveOptionsMenu = optionsMenu != null;

            if (!hadOptionsMenu && haveOptionsMenu) {
                Melon<Core>.Logger.Msg("have options menu");
                CreateFrameStatsSettingsUI();
            } else if (hadOptionsMenu && !haveOptionsMenu) {
                Melon<Core>.Logger.Msg("lost options menu");
            }

            hadOptionsMenu = haveOptionsMenu;
            if (inGameMenu && !inGameMenu.activeSelf) PrintStats();
        }

        private void PrintStats() {
            Melon<Core>.Logger.Msg("hello from performance monitor");
        }

        private void CreateFrameStatsSettingsUI() {
            GameObject frameStatsSettings = new GameObject("FrameStats");
            frameStatsSettings.transform.SetParent(optionsMenu.transform);
            Melon<Core>.Logger.Msg("added stats setting option");
        }
    }
}