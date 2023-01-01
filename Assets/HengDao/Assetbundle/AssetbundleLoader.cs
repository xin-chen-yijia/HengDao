using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.IO;

namespace HengDao
{
    //每个工程打一个assetbundle包，在这个类中要处理多个工程打的多个包，从多个包中找到具体资源加载
    public class AssetBundleLoader
    {
        public const string kDynamicAssetBundleName = "Dynamic";
        public const string kpandaVariantName = "panda";

        private string assetbundlesDir_ = string.Empty;  
        public string assetbundleDir
        {
            get
            {
                return assetbundlesDir_;
            }
        }

        public AssetBundleManifest mainManifest_ = null;
        public Dictionary<string, AssetBundle> loadedBundles_ = new Dictionary<string, AssetBundle>();
        private string curVariantName_ = kpandaVariantName;

        private string prefix_ = "";    //名字前缀
        private string dynamicABName = "";

        public bool Init(string path)
        {
            if(!Directory.Exists(path))
            {
                HengDao.Logger.Error(path + " not exists..");
                return false;
            }

            assetbundlesDir_ = path;
            prefix_ = GetPrefixStr(path);
            dynamicABName = prefix_ + kDynamicAssetBundleName.ToLower() + "." + curVariantName_;
            string folder = Path.GetFileName(path);
            AssetBundle ab = AssetBundle.LoadFromFile(Path.Combine(path, folder));
            mainManifest_ = ab.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            ab.Unload(false);

            return true;
        }

        //~AssetBundleLoader()
        //{
        //    //foreach (var v in mLoadedAssetBundles)
        //    //{
        //    //    //v.Value.Unload(false);
        //    //}
        //}

        public void SetCurrentVariantName(string variantName)
        {
            curVariantName_ = variantName;
        }

        public T Load<T>(string assetName) where T : UnityEngine.Object
        {
            string bundleName = dynamicABName; 
            AssetBundle ab = GetOrLoadBundle(bundleName);
            if (ab != null)
            {
                T t = ab.LoadAsset<T>(assetName);
                return t;
            }

            Logger.Error(string.Format("LoadAsset Error.bundleName:{0},assetName:{1}", bundleName, assetName));
            return null;
        }

        public void LoadAsync<T>(string assetName, System.Action<T> onLoaded) where T : UnityEngine.Object
        {
            string bundleName = dynamicABName;
            LoadAssetBundleAsyn(bundleName, (AssetBundle bundle) =>
            {
                T t = bundle.LoadAsset<T>(assetName);
                
                if(onLoaded != null)
                {
                    onLoaded(t);
                }
            });
        }

        public T LoadAndInstantiate<T>(string assetName, string instantiateName = "", Transform parent = null) where T : UnityEngine.Object
        {
            T t = Load<T>(assetName);
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

        public AssetBundle LoadAssetBundle(string bundleName)
        {
            bundleName = prefix_ + bundleName.ToLower() + "." + curVariantName_;
            return GetOrLoadBundle(bundleName);
        }

        public void LoadAssetBundleAsyn(string bundleName, System.Action<AssetBundle> onLoadedAssetBundle)
        {
            bundleName = prefix_ + bundleName.ToLower() + "." + curVariantName_;
            CoroutineLauncher.current.StartCoroutine(LoadAssetBundleCoroutine(bundleName, onLoadedAssetBundle));
        }

        //具体某个assetbundle
        private AssetBundle GetOrLoadBundle(string bundleName)
        {
            if (!loadedBundles_.ContainsKey(bundleName))
            {
                string[] dps = mainManifest_.GetAllDependencies(bundleName);
                foreach (var v in dps)
                {
                    GetOrLoadBundle(v);
                }

                loadedBundles_[bundleName] = AssetBundle.LoadFromFile(System.IO.Path.Combine(assetbundlesDir_, bundleName));
            }

            return loadedBundles_[bundleName];
        }

        private IEnumerator LoadAssetBundleCoroutine(string bundleName, System.Action<AssetBundle> onLoadedAssetBundle)
        {
            if (!loadedBundles_.ContainsKey(bundleName))
            {
                string[] dps = mainManifest_.GetAllDependencies(bundleName);
                foreach (var v in dps)
                {
                    yield return LoadAssetBundleCoroutine(v, (bundle)=>
                    {
                        Logger.Info("loaded " + bundle.name);
                    });
                }

                AssetBundleCreateRequest ab = AssetBundle.LoadFromFileAsync(System.IO.Path.Combine(assetbundlesDir_, bundleName));
                yield return ab;
                if (ab == null)
                {
                    Logger.Error("Load AssetBundle " + bundleName + " Fail...");
                    yield break;
                }

                loadedBundles_[bundleName] = ab.assetBundle;
                onLoadedAssetBundle?.Invoke(ab.assetBundle);
            }
        }

        public void Unload(bool unloadAllLoadedObjects)
        {
            foreach(var v in loadedBundles_)
            {
                v.Value.Unload(unloadAllLoadedObjects);
            }
        }

        private static readonly Dictionary<Type, string> extentionTypes = new Dictionary<Type, string>()
        {
            {typeof(Texture),".png" },
            {typeof(GameObject),".prefab" },
            {typeof(Sprite),".png" }
        };
        private string GetExtension<T>()
        {
            string res;
            extentionTypes.TryGetValue(typeof(T), out res);
            return res;
        }

        public static string GetPrefixStr(string path)
        {
            return Path.GetFileName(path).ToLower() + "_";
        }

    }
}