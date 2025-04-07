using MelonLoader;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FrameStats {
    public class AssetLoader {
        private readonly Dictionary<string, Object> _assets;
        private readonly MelonLogger.Instance _logger;

        public AssetLoader(MelonLogger.Instance logger, string assetBundlesRoot) {
            _logger = logger;

            foreach (string filePath in Directory.GetFiles(assetBundlesRoot)) {
                string assetBundleName = Path.GetFileNameWithoutExtension(filePath);
                AssetBundle assetBundle = AssetBundle.LoadFromFile(filePath);

                if (!assetBundle) {
                    _logger.Error($"failed to load asset bundle {assetBundleName}");
                    continue;
                }

                _logger.Msg($"Loading asset bundle {assetBundleName}...");
                _assets = new Dictionary<string, Object>();
                foreach (Object asset in assetBundle.LoadAllAssets()) {
                    _logger.Msg($"    Loaded asset {asset.name}");
                    _assets.Add($"{assetBundleName}/{asset.name}", asset);
                }
            }
        }

        public Object GetAsset(string assetPath) {
            return Object.Instantiate(_assets[assetPath]);
        }
    }
}