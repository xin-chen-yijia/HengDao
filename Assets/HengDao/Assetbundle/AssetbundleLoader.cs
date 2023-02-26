using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using Newtonsoft.Json;

namespace HengDao
{

    public class AssetBundleLauchDesc
    {
        public string version = string.Empty;
        public List<AssetBundleContent> contents = new List<AssetBundleContent>();
        public List<string> plugins = new List<string>();
    }

    //每个工程打一个assetbundle包，在这个类中要处理多个工程打的多个包，从多个包中找到具体资源加载
    public class AssetBundleLoader
    {

        // AB包存放路径
        private string assetbundlesDir_ = string.Empty;  
        public string assetbundleDir
        {
            get
            {
                return assetbundlesDir_;
            }
        }

        private AssetBundleLauchDesc assetbundleLauchDesc_ = null; //assetbundle 描述信息

        private AssetBundleManifest mainManifest_ = null;
        private Dictionary<string, AssetBundle> loadedBundles_ = new Dictionary<string, AssetBundle>();
        private string curVariantName_ = "";

        public bool Init(string path, string variantName= AssetBundlePresets.kPandaVariantName)
        {
            curVariantName_ = variantName;

            if (!Directory.Exists(path))
            {
                HengDao.Logger.Error(path + " not exists..");
                return false;
            }

            assetbundlesDir_ = path;
            if(assetbundlesDir_.EndsWith("/") || assetbundlesDir_.EndsWith(@"\"))
            {
                assetbundlesDir_ = assetbundlesDir_.Substring(0, assetbundlesDir_.Length - 1);
            }

            return true;
        }

        private string JointAssetBundleAndVariantName(string assetbundleName)
        {
            return assetbundleName + "." + curVariantName_;
        }

        public string GetAssetBundleWithAssetName(string assetName)
        {
            if(assetbundleLauchDesc_ != null)
            {
                foreach(var v in assetbundleLauchDesc_.contents)
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

        //~AssetBundleLoader()
        //{
        //    //foreach (var v in mLoadedAssetBundles)
        //    //{
        //    //    //v.Value.Unload(false);
        //    //}
        //}

        public T LoadAsset<T>(string assetName) where T : UnityEngine.Object
        {
            string bundleName = GetAssetBundleWithAssetName(assetName);
            AssetBundle ab = LoadBundle(bundleName);
            if (ab != null)
            {
                T t = ab.LoadAsset<T>(assetName);
                return t;
            }

            Logger.Error(string.Format("LoadAsset Error.bundleName:{0},assetName:{1}", bundleName, assetName));
            return null;
        }

        public T LoadAssetAndInstantiate<T>(string assetName, string instantiateName = "", Transform parent = null) where T : UnityEngine.Object
        {
            T t = LoadAsset<T>(assetName);
            if (t != null)
            {
                T it = GameObject.Instantiate<T>(t,parent);
                if (!string.IsNullOrEmpty(instantiateName))
                {
                    it.name = instantiateName;
                }

                return it;
            }

            return null;
        }



        //具体某个assetbundle
        public AssetBundle LoadBundle(string bundleName)
        {
            if (!mainManifest_)
            {
                string baseAbName = Path.GetFileName(assetbundlesDir_);
                AssetBundle baseAb = AssetBundle.LoadFromFile(Path.Combine(assetbundlesDir_, baseAbName));
                mainManifest_ = baseAb.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
                baseAb.Unload(false);
            }
            if (!loadedBundles_.ContainsKey(bundleName))
            {
                string[] dps = mainManifest_.GetAllDependencies(bundleName);
                foreach (var v in dps)
                {
                    LoadBundle(v);
                }

                loadedBundles_[bundleName] = AssetBundle.LoadFromFile(System.IO.Path.Combine(assetbundlesDir_, bundleName));
            }

            return loadedBundles_[bundleName];
        }


        public void UnloadAssetBundles(bool unloadAllLoadedObjects)
        {
            foreach (var v in loadedBundles_)
            {
                v.Value.Unload(unloadAllLoadedObjects);
            }

            loadedBundles_.Clear();
        }

        /// <summary>
        /// 获取 assetbundle 的描述文件
        /// </summary>
        /// <returns></returns>
        public bool LoadAssetBundleLaunchConfig()
        {
            string configContent = string.Empty;
            string filePath = Path.Combine(assetbundlesDir_, AssetBundlePresets.kLauchDescFileName);
            if (File.Exists(filePath))
            {
                configContent = File.ReadAllText(filePath);
            }
            else
            {
                Logger.Warning(filePath + " not exists...");
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
        public bool CheckPluginRequirements(string path = "AssetBundlePluginRequirements")
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
    }
}