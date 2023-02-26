using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

namespace HengDao
{

    public class AsyncAssetBundleLoader
    {
        // ab包路径
        private string assetbundlesDir_ = string.Empty;
        public string assetbundleDir
        {
            get
            {
                return assetbundlesDir_;
            }
        }

        private bool isWebRequest_ = false;    // 是否用UnityWebRequestAssetBundle加载assetbunle

        private AssetBundleLauchDesc assetbundleLauchDesc_ = null; //assetbundle 描述信息

        private AssetBundleManifest mainManifest_ = null;
        private Dictionary<string, AssetBundle> loadedBundles_ = new Dictionary<string, AssetBundle>();
        private string curVariantName_ = "";

        public async Task<bool> Init(string path, string variantName = AssetBundlePresets.kPandaVariantName)
        {
            if (path.StartsWith("http://") || path.StartsWith("www.") || path.StartsWith("file:///"))
            {
                isWebRequest_ = true;
            }

            assetbundlesDir_ = path;
            if (assetbundlesDir_.EndsWith("/") || assetbundlesDir_.EndsWith(@"\"))
            {
                assetbundlesDir_ = assetbundlesDir_.Substring(0, assetbundlesDir_.Length - 1);
            }

            var res = await LoadAssetBundleLaunchConfig();
            
            return res && CheckPluginRequirements();
        }
        public async Task<T> LoadAssetAsync<T>(string assetName) where T : UnityEngine.Object
        {
            string bundleName = GetAssetBundleWithAssetName(assetName);
            var bundle = await LoadAssetBundleAsync(bundleName);
            return bundle.LoadAsset<T>(assetName);
        }

        private async Task<AssetBundle> LoadAssetbundleAsync_web(string path)
        {
            var request = UnityEngine.Networking.UnityWebRequestAssetBundle.GetAssetBundle(path, 0);
            await request.SendWebRequest();
            AssetBundle bundle = UnityEngine.Networking.DownloadHandlerAssetBundle.GetContent(request);
            if (bundle)
            {
                Logger.Info("Loaded assetbundle:" + path);
            }
            else
            {
                Logger.Error("load assetbundle:" + path + " failed..");
            }
            return bundle;
        }

        private async Task<AssetBundle> LoadAssetbundleAsync_file(string path)
        {
            var requestOp = AssetBundle.LoadFromFileAsync(path);
            await requestOp;
            AssetBundle bundle = requestOp.assetBundle;
            if (bundle)
            {
                Logger.Info("Loaded assetbundle:" + path);
            }
            else
            {
                Logger.Error("load assetbundle:" + path + " failed..");
            }

            return bundle;
        }

        public async Task<AssetBundle> LoadAssetBundleAsync(string bundleName)
        {
            if (string.IsNullOrEmpty(bundleName))
            {
                Logger.Error("LoadAssetBundleAsync: empty name");
                return null;
            }

            System.Func<string, Task<AssetBundle>> LoadAssetbundleFunc = isWebRequest_ ? LoadAssetbundleAsync_web : LoadAssetbundleAsync_file;

            if (!mainManifest_)
            {
                string baseAbName = Path.GetFileName(assetbundlesDir_);
                string manifestBundlePath = Path.Combine(assetbundlesDir_, baseAbName);

                AssetBundle manifestBundle = await LoadAssetbundleFunc(manifestBundlePath);
                mainManifest_ = manifestBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
                manifestBundle.Unload(false);
            }
            if (!loadedBundles_.ContainsKey(bundleName))
            {
                string[] dps = mainManifest_.GetAllDependencies(bundleName);
                foreach (var v in dps)
                {
                    await LoadAssetBundleAsync(v);
                }

                string bundlePath = System.IO.Path.Combine(assetbundlesDir_, bundleName);

                loadedBundles_[bundleName] = await LoadAssetbundleFunc(bundlePath);
            }

            return loadedBundles_.ContainsKey(bundleName) ? loadedBundles_[bundleName] : null;
        }

        /// <summary>
        /// 获取 assetbundle 的描述文件
        /// </summary>
        /// <returns></returns>
        private async Task<bool> LoadAssetBundleLaunchConfig()
        {
            string configContent = string.Empty;
            if (isWebRequest_)
            {
                UnityWebRequest www = UnityWebRequest.Get(assetbundlesDir_ + "/" + AssetBundlePresets.kLauchDescFileName);
                await www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Logger.Error(www.error);
                }
                else
                {
                    configContent = www.downloadHandler.text;
                }
            }
            else
            {
                System.Func<Task<object>> tawaiter = async () =>
                {
                    string filePath = Path.Combine(assetbundlesDir_, AssetBundlePresets.kLauchDescFileName);
                    if (File.Exists(filePath))
                    {
                        configContent = File.ReadAllText(filePath);

                    }
                    else
                    {
                        Logger.Warning(filePath + " not exists...");
                    }

                    return new object();
                };
                await tawaiter();
            }

            try
            {
                // Show results as text
                assetbundleLauchDesc_ = JsonConvert.DeserializeObject<AssetBundleLauchDesc>(configContent);
                Debug.Log(configContent + ":" + assetbundleLauchDesc_);
                int curVersion = 0;
                int.TryParse(Application.unityVersion.Substring(0, Application.unityVersion.IndexOf(".")), out curVersion);
                int abVersion = -1;
                int.TryParse(assetbundleLauchDesc_.version.Substring(0, assetbundleLauchDesc_.version.IndexOf(".")), out abVersion);
                if (assetbundleLauchDesc_ == null || abVersion < curVersion)
                {
                    Logger.Error("AssetBundle build info format error or build with old version:" + assetbundleLauchDesc_.version);
                    return false;
                }
                else
                {
                    return true;
                }

            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                return false;
            }
        }

        /// <summary>
        /// 检查插件引用
        /// </summary>
        /// <returns></returns>
        private bool CheckPluginRequirements(string path = "AssetBundlePluginRequirements")
        {
            ResourceLoader loader = new ResourceLoader();
            var req = loader.Load<AssetBundlePluginRequirement>(path);
            if (!req)
            {
                return false;
            }

            bool res = true;
            for (int i = 0; res && i < req.plugins.Count; ++i)
            {
                res = assetbundleLauchDesc_.plugins.Contains(req.plugins[i]);
            }
            return res;
        }

        public string GetAssetBundleWithAssetName(string assetName)
        {
            if (assetbundleLauchDesc_ != null)
            {
                foreach (var v in assetbundleLauchDesc_.contents)
                {
                    foreach (var name in v.assets)
                    {
                        string fileName = name.Substring(name.LastIndexOf("/") + 1);
                        if (fileName.Contains(assetName))
                        {
                            return v.assetbundleName;
                        }
                    }
                }
            }

            return "";
        }
    }
}
