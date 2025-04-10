using MelonLoader;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FrameStats {
    public class AssetNotFoundException : Exception {
        public AssetNotFoundException() {}
        public AssetNotFoundException(string message) : base(message) {}
        public AssetNotFoundException(string message, Exception innerException) : base(message, innerException) {}
    }

    public class AssetManager {
        private Dictionary<string, Object> _assets;

        public AssetManager(string assetBundlesRoot) {
            _assets = new Dictionary<string, Object>();
            foreach (string assetBundlePath in Directory.GetFiles(assetBundlesRoot)) {
                AssetBundle assetBundle = AssetBundle.LoadFromFile(assetBundlePath);
                if (!assetBundle) {
                    // May not be an asset bundle so just leave a warning
                    Melon<Core>.Logger.Warning($"failed to load asset bundle from file {Path.GetFileName(assetBundlePath)}");
                    continue;
                }

                string assetBundleName = Path.GetFileNameWithoutExtension(assetBundlePath);
                Melon<Core>.Logger.Msg($"Loading asset bundle {assetBundleName}...");
                foreach (Object asset in assetBundle.LoadAllAssets()) {
                    string assetPath = $"{assetBundleName}/{asset.name}";
                    _assets.Add(assetPath, asset);
                    Melon<Core>.Logger.Msg($"Loaded asset {assetPath}");
                }
            }
        }

        public T InstantiateAsset<T>(string assetPath) where T : Object {
            if (!_assets.ContainsKey(assetPath)) {
                throw new AssetNotFoundException($"asset {assetPath} not found");
            }

            return Object.Instantiate(_assets[assetPath]) as T;
        }
    }
}