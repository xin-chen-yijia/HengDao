using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace HengDao
{
    //每个工程打一个assetbundle包，在这个类中要处理多个工程打的多个包，从多个包中找到具体资源加载
    public class AssetBundleLoader : IAssetLoader
    {

        private string mPath = string.Empty;

        //存储多个工程打的包的信息
        private Dictionary<string, LoaderContext> contexts_ = new Dictionary<string, LoaderContext>();

        //多个assetbundle的情况
        private class LoaderContext
        {
            public string name;
            public AssetBundleManifest manifest_ = null;
            public Dictionary<string, AssetBundle> loadedBundles = new Dictionary<string, AssetBundle>();
        }


        public AssetBundleLoader(string path)
        {
            mPath = path.Replace('\\', '/');
        }

        //~AssetBundleLoader()
        //{
        //    //foreach (var v in mLoadedAssetBundles)
        //    //{
        //    //    //v.Value.Unload(false);
        //    //}
        //}

        public T Load<T>(string assetName) where T : UnityEngine.Object
        {
            assetName += GetExtension<T>();
            string bundleName, setName;
            if (!AssetBundleSetUtil.GetAssetOwnerInfo(assetName, out setName, out bundleName))
            {
                return null;
            }
            LoaderContext context = GetLoaderContext(setName);

            Debug.Assert(context != null);
            AssetBundle ab = GetOrLoadBundle(context, bundleName);
            if (ab != null)
            {
                T t = ab.LoadAsset<T>(assetName);
                return t;
            }

            Debug.LogError(string.Format("LoadAsset Error.bundleName:{0},assetName:{1}", bundleName, assetName));
            return null;
        }

        public void LoadAsync<T>(string assetName, System.Action<T> onLoaded) where T : UnityEngine.Object
        {
            assetName += GetExtension<T>();
            string bundleName, setName;
            if (!AssetBundleSetUtil.GetAssetOwnerInfo(assetName, out setName, out bundleName))
            {
                onLoaded?.Invoke(null);
                return;
            }

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

        public void LoadBundle(string bundleName)
        {
            string setName = AssetBundleSetUtil.GetSetNameWithBundle(bundleName);
            LoaderContext context = GetLoaderContext(setName);

            Debug.Assert(context != null);
            GetOrLoadBundle(context, bundleName);
        }

        public void LoadAssetBundleAsyn(string bundleName, System.Action<AssetBundle> onLoadedAssetBundle)
        {
            CoroutineLauncher.current.StartCoroutine(LoadAssetBundleCoroutine(bundleName, onLoadedAssetBundle));
        }

        //获取bundle在哪个package中
        private LoaderContext GetLoaderContext(string name)
        {
            if(!System.IO.File.Exists(mPath + "/" + name + "/" + name))
            {
                Debug.LogError("not exist assetbundle:" + name);
                return null;
            }
            if(!contexts_.ContainsKey(name))
            {
                AssetBundle ab = AssetBundle.LoadFromFile(WrapAssetBundlePath(name + "/" + name));
                AssetBundleManifest manifest = ab.LoadAsset<AssetBundleManifest>("AssetBundleManifest");

                LoaderContext context = new LoaderContext();
                context.name = name;
                context.manifest_ = manifest;

                ab.Unload(false);

                contexts_[name] = context;
            }

            return contexts_[name];
        }

        //具体某个assetbundle
        private AssetBundle GetOrLoadBundle(LoaderContext context, string bundleName)
        {
            if (!context.loadedBundles.ContainsKey(bundleName))
            {
                string[] dps = context.manifest_.GetAllDependencies(bundleName);
                foreach (var v in dps)
                {
                    GetOrLoadBundle(context,v);
                }

                context.loadedBundles[bundleName] = AssetBundle.LoadFromFile(WrapAssetBundlePath(context.name + "/" + bundleName));
            }

            return context.loadedBundles[bundleName];
        }

        private IEnumerator LoadAssetBundleCoroutine(string bundleName, System.Action<AssetBundle> onLoadedAssetBundle)
        {
            string setName = AssetBundleSetUtil.GetSetNameWithBundle(bundleName);
            LoaderContext context = GetLoaderContext(setName);

            Debug.Assert(context != null);

            if (!contexts_[setName].loadedBundles.ContainsKey(bundleName))
            {
                string[] dps = contexts_[setName].manifest_.GetAllDependencies(bundleName);
                foreach (var v in dps)
                {
                    yield return LoadAssetBundleCoroutine(v, (bundle)=>
                    {
                        Debug.Log("loaded " + bundle.name);
                    });
                }

                yield return LoadABCoroutine(context.name + "/" +bundleName, (bundle)=>
                {
                    contexts_[setName].loadedBundles[bundleName] = bundle;
                    onLoadedAssetBundle?.Invoke(bundle);
                });
            }
            else
            {
                onLoadedAssetBundle?.Invoke(contexts_[setName].loadedBundles[bundleName]);
            }
        }

        private IEnumerator LoadABCoroutine(string bundlePath, System.Action<AssetBundle> onLoadedAssetBundle)
        {
            AssetBundleCreateRequest ab = AssetBundle.LoadFromFileAsync(WrapAssetBundlePath(bundlePath));
            yield return ab;
            if (ab == null)
            {
                Debug.Log("Load AssetBundle " + bundlePath + " Fail...");
                yield break;
            }

            onLoadedAssetBundle?.Invoke(ab.assetBundle);
        }

        public void LoadScene(string scene, System.Action onLoaded)
        {
            scene += ".unity";
            string setName, bundleName;
            if(!AssetBundleSetUtil.GetAssetOwnerInfo(scene,out setName,out bundleName))
            {
                return;
            }
            LoadAssetBundleAsyn(bundleName, (AssetBundle bundle)=>
            {
                onLoaded();
            });
        }

        private string WrapAssetBundlePath(string bundleName)
        {
            return System.IO.Path.Combine(mPath, bundleName);
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

    }
}