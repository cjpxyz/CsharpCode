using AssetBundleManagement;
using UnityEngine;

namespace AssetBundleManager.Operation
{
    class AssetSimulatedLoading : AssetLoading
    {
        public AssetSimulatedLoading(string bundleName, string assetName)
            : base(AssetLoadingPattern.Simulation, bundleName, assetName)
        { }

        public override bool IsDone()
        {
            return true;
        }

        public override void Process()
        { }

        public override void SetAssetBundle(LoadedAssetBundle assetBundle)
        { }

        public void SetAsset(Object obj)
        {
            LoadedAsset = obj;
        }
    }
}