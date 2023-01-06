using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using UnityEngine.Networking;

namespace HengDao
{
    public class AssetBundlePresets
    {
        public const string kAssetBundleName = "Dynamic";
        public const string kVariantName = "panda";
        public const string kBuildInfoFileName = "assetbundle_build.json";
    }

    public class AssetBundleBuildInfo
    {
        public string version;
    }

    //每个工程打一个assetbundle包，在这个类中要处理多个工程打的多个包，从多个包中找到具体资源加载
    public class AssetBundleLoader
    {

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
        private string curVariantName_ = "";

        private string abNamePrefix_ = "";    //名字前缀
        private string dynamicABName_ = "";

        private bool isAsyncRequest_ = false;    // 是否用UnityWebRequestAssetBundle加载assetbunle
        private int assetBundleCheckedState_ =  0;    // ab包检测状态，位运算，bits[0] 表示是否检测，bits[1]表示是否有效，0表示未检测，3表示有效

        public enum AssetBundleError
        {
            kSucess = 0,
            kIncomplete,
            kAssetBundleVersonOld,
            kDownloadFail,
        }

        public bool Init(string path, string variantName= AssetBundlePresets.kVariantName)
        {
            curVariantName_ = variantName;

            if(path.StartsWith("http://") || path.StartsWith("file:///") || path.StartsWith("www."))
            {
                isAsyncRequest_ = true;
            }
            else
            {
                if (!Directory.Exists(path))
                {
                    HengDao.Logger.Error(path + " not exists..");
                    return false;
                }
            }
            
            assetbundlesDir_ = path;
            if(assetbundlesDir_.EndsWith("/") || assetbundlesDir_.EndsWith(@"\"))
            {
                assetbundlesDir_ = assetbundlesDir_.Substring(0, assetbundlesDir_.Length - 1);
            }
            abNamePrefix_ = ParseABNamePrefixFromPath(path);

            Logger.Info("Loader' assetbundle's dir:" + assetbundleDir);
            // dynamic assetbundle
            dynamicABName_ = abNamePrefix_ + AssetBundlePresets.kAssetBundleName.ToLower() + "." + curVariantName_;

            return true;
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
#if UNITY_EDITOR
            if(isAsyncRequest_)
            {
                Logger.Error("loader use UnityWebRequestAssetBundle. please use LoadAssetAsync instead.");
                return null;
            }
#endif
            string bundleName = dynamicABName_; 
            AssetBundle ab = GetOrLoadBundle(bundleName);
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
#if UNITY_EDITOR
            if (isAsyncRequest_)
            {
                Logger.Error("loader use UnityWebRequestAssetBundle. please use LoadAssetAsync instead.");
                return null;
            }
#endif
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

        public void LoadAssetAsync<T>(string assetName, System.Action<T> onLoaded, System.Action<string> onError) where T : UnityEngine.Object
        {
            string bundleName = dynamicABName_;
            CoroutineLauncher.current.StartCoroutine(GetOrLoadAssetBundleAsyn(bundleName, (AssetBundle bundle) =>
            {
                T t = bundle.LoadAsset<T>(assetName);

                if (onLoaded != null)
                {
                    onLoaded(t);
                }
            },null));
        }

        //具体某个assetbundle
        private AssetBundle GetOrLoadBundle(string bundleName)
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
                    GetOrLoadBundle(v);
                }

                loadedBundles_[bundleName] = AssetBundle.LoadFromFile(System.IO.Path.Combine(assetbundlesDir_, bundleName));
            }

            return loadedBundles_[bundleName];
        }


        public IEnumerator GetOrLoadAssetBundleAsyn(string bundleName, System.Action<AssetBundle> onLoadedAssetBundle, System.Action<string> onError)
        {
            if (!mainManifest_)
            {
                string baseAbName = Path.GetFileName(assetbundlesDir_);
                string manifestBundlePath = Path.Combine(assetbundlesDir_, baseAbName);
                var manifestRequest = UnityEngine.Networking.UnityWebRequestAssetBundle.GetAssetBundle(manifestBundlePath, 0);
                yield return manifestRequest.SendWebRequest();
                Logger.Info("Loaded assetbundle:" + manifestBundlePath);
                AssetBundle manifestBundle = UnityEngine.Networking.DownloadHandlerAssetBundle.GetContent(manifestRequest);
                mainManifest_ = manifestBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
                manifestBundle.Unload(false);
            }
            if (!loadedBundles_.ContainsKey(bundleName))
            {
                string[] dps = mainManifest_.GetAllDependencies(bundleName);
                foreach (var v in dps)
                {
                    yield return GetOrLoadAssetBundleAsyn(v,null,null);
                }

                string bundlePath = System.IO.Path.Combine(assetbundlesDir_, bundleName);
                var bundleRequest = UnityEngine.Networking.UnityWebRequestAssetBundle.GetAssetBundle(bundlePath, 0);
                yield return bundleRequest.SendWebRequest();
                Logger.Info("Loaded assetbundle:" + bundlePath);
                loadedBundles_[bundleName] = UnityEngine.Networking.DownloadHandlerAssetBundle.GetContent(bundleRequest);

            }

            if(onLoadedAssetBundle != null)
            {
                onLoadedAssetBundle(loadedBundles_[bundleName]);
            }
        }

        public void Unload(bool unloadAllLoadedObjects)
        {
            foreach(var v in loadedBundles_)
            {
                v.Value.Unload(unloadAllLoadedObjects);
            }

            loadedBundles_.Clear();
        }

        public void CheckAssetBundleValid(System.Action<AssetBundleError, string> onError)
        {
            CoroutineLauncher.current.StartCoroutine(CheckAssetBundleCoroutine(onError));    
        }

        private IEnumerator CheckAssetBundleCoroutine(System.Action<AssetBundleError, string> onError)
        {
            if(isAsyncRequest_)
            {
                UnityWebRequest www = UnityWebRequest.Get(assetbundlesDir_ + "/" + AssetBundlePresets.kBuildInfoFileName);
                yield return www.SendWebRequest();

                if (www.isHttpError || www.isNetworkError)
                {
                    Logger.Error(www.error);
                    onError(AssetBundleError.kDownloadFail, www.error);
                }
                else
                {
                    try
                    {
                        // Show results as text
                        var tmp = JsonUtility.FromJson<AssetBundleBuildInfo>(www.downloadHandler.text);
                        int curVersion = 0;
                        int.TryParse(Application.unityVersion.Substring(0, Application.unityVersion.IndexOf(".")), out curVersion);
                        int abVersion = -1;
                        int.TryParse(tmp.version.Substring(0, tmp.version.IndexOf(".")), out abVersion);
                        if (tmp == null || abVersion < curVersion)
                        {
                            onError?.Invoke(AssetBundleError.kAssetBundleVersonOld, "AssetBundle build info format error or build with old version:" + tmp.version);
                        }
                        else
                        {
                            onError?.Invoke(AssetBundleError.kSucess, "");
                        }
                    }catch(Exception e)
                    {
                        onError(AssetBundleError.kIncomplete, e.Message);
                    }
                   
                }
            }
            else
            {
                yield return null;
                string filePath = Path.Combine(assetbundlesDir_, AssetBundlePresets.kBuildInfoFileName);
                if(File.Exists(filePath))
                {
                    try
                    {
                        var tmp = JsonUtility.FromJson<AssetBundleBuildInfo>(File.ReadAllText(filePath));
                        int curVersion = 0;
                        int.TryParse(Application.unityVersion.Substring(0, Application.unityVersion.IndexOf(".")), out curVersion);
                        int abVersion = -1;
                        int.TryParse(tmp.version.Substring(0, tmp.version.IndexOf(".")), out abVersion);
                        if (tmp == null || abVersion < curVersion)
                        {
                            onError?.Invoke(AssetBundleError.kAssetBundleVersonOld, "AssetBundle build info format error or build with old version:" + tmp.version);
                        }
                        else
                        {
                            onError?.Invoke(AssetBundleError.kSucess, "");
                        }
                    }
                    catch(Exception e)
                    {
                        onError(AssetBundleError.kIncomplete, e.Message);
                    }
                }
                else
                {
                    Logger.Warning(filePath + " not exists...");
                    onError?.Invoke(AssetBundleError.kIncomplete, filePath + " not exists...");
                }
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

        public static string ParseABNamePrefixFromPath(string path)
        {
            return Path.GetFileName(path).ToLower() + "_";
        }

    }
}