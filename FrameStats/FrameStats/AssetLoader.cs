using MelonLoader;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FrameStats {
    public class AssetLoader {
        private Dictionary<string, Object> _assets;

        public AssetLoader(string assetBundlesRoot) {
            foreach (string filePath in Directory.GetFiles(assetBundlesRoot)) {
                string assetBundleName = Path.GetFileNameWithoutExtension(filePath);
                AssetBundle assetBundle = AssetBundle.LoadFromFile(filePath);

                if (!assetBundle) {
                    Melon<Core>.Logger.Error($"failed to load asset bundle {assetBundleName}");
                    continue;
                }

                Melon<Core>.Logger.Msg($"Loading asset bundle {assetBundleName}...");
                _assets = new Dictionary<string, Object>();
                foreach (Object asset in assetBundle.LoadAllAssets()) {
                    string assetPath = $"{assetBundleName}/{asset.name}";
                    Melon<Core>.Logger.Msg($"Loaded asset {assetPath}");
                    _assets.Add(assetPath, asset);
                }
            }
        }

        public Object InstantiateAsset(string assetPath) {
            if (!_assets.ContainsKey(assetPath)) return null;
            return Object.Instantiate(_assets[assetPath]);
        }
    }
}