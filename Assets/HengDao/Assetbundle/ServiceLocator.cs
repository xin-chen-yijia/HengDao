using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HengDao
{
    /// <summary>
    /// servcie locator pattern
    /// </summary>
    public class ServiceLocator
    {
        private static IAssetLoader assetLoader_;

        private static string s_ab_path = string.Empty;
        static ServiceLocator()
        {

        }

        public static bool Init(string configPath)
        {
            if(!System.IO.File.Exists(configPath))
            {
                Logger.Log(configPath + " not exists...");
                return false;
            }

            AssetBundleSetUtil.InitConfigFile(configPath);

#if Debug
            assetLoader_ = new ResourceLoader();
#else
            assetLoader_ = new AssetBundleLoader(AssetBundleSetUtil.assetbundlesDir);
#endif
            return true;
        }

        public static IAssetLoader GetABLoader()
        {
            if (assetLoader_ == null)
            {
                Logger.Error("ABManager Not Initialize...");
            }
            return assetLoader_;
        }
    }

}
