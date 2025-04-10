using MelonLoader;
using UI.Common.Options;
using UI.FrameStats;
using UnityEngine;

[assembly: MelonInfo(typeof(FrameStats.Core), "FrameStats", "1.0.0", "oneshade", null)]
[assembly: MelonGame("CuriousOwlGames", "StellarDrive")]

namespace FrameStats {
    public class Core : MelonMod {
        private AssetManager _assetManager;
        private GameObject _activeMenuContainer = null;
        private GameObject _optionsMenu = null;
        private bool _hadOptionsMenu = false;

        public override void OnInitializeMelon() {
            MelonPreferences_Category frameStatsPreferences = MelonPreferences.CreateCategory("FrameStatsPreferences");
            MelonPreferences_Entry<bool> frameStatsEnabled = frameStatsPreferences.CreateEntry<bool>("FrameStatsEnabled", false);
            _assetManager = new AssetManager("./UserData/AssetBundles");
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName) {
            Melon<Core>.Logger.Msg($"scene {sceneName} loaded, index {buildIndex}");

            switch (buildIndex) {
                case 0: // MainMenu
                    _activeMenuContainer = GameObject.Find("MainMenuCanvas/MainMenu/Menu/SelectedMenu/MenuContent");
                    break;

                case 1: // MainGame
                    _activeMenuContainer = GameObject.Find("UI/PauseCanvas/Canvas/Menu");
                    CreateFrameStatsPanelUI();
                    break;

                default:
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
        }

        private void CreateFrameStatsSettingsUI() {
            GameObject frameStatsSettings = _assetManager.InstantiateAsset<GameObject>("ui/FrameStatsSettings");
            frameStatsSettings.name = "FrameStats";
            frameStatsSettings.AddComponent<FrameStatsSettings>();
            frameStatsSettings.transform.SetParent(_optionsMenu.transform);
            frameStatsSettings.transform.localScale = Vector3.one;
            Melon<Core>.Logger.Msg("added frame stats settings");
        }

        private void CreateFrameStatsPanelUI() {
            GameObject frameStatsPanel = _assetManager.InstantiateAsset<GameObject>("ui/FrameStatsPanel");
            frameStatsPanel.name = "FrameStatsPanel";
            frameStatsPanel.AddComponent<FrameStatsUpdater>();
            frameStatsPanel.transform.SetParent(GameObject.Find("UI")?.transform);
            frameStatsPanel.transform.localScale = Vector3.one;
            Melon<Core>.Logger.Msg("added frame stats panel");
        }
    }
}