using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HengDao
{
    public class LoadService  
    {
        private IAssetLoader loader_ = null;

        private string dir_;
        public LoadService(string path)
        {
            dir_ = path;           
        }

        private IAssetLoader GetOrCreateLoader(string loaderName)
        {
            if(loader_ == null)
            {
#if Debug
                loader_ = new ResourceLoader();
#else
                loader_ = new AssetBundleLoader(dir_ + "/" + loaderName);
#endif
            }

            return loader_;
        }

        public void LoadAssetBundleAsyn(string bundleName,Action<AssetBundle> onLoadedAssetBundle)
        {
            string setName = AssetBundleSetUtil.GetSetNameWithBundle(bundleName);
            if(!string.IsNullOrEmpty(setName))
            {
                var loader = GetOrCreateLoader(setName);
                //loader.LoadAssetBundleAsyn(bundleName, onLoadedAssetBundle);
            }
            else
            {
                Debug.LogError(bundleName + " can't found..");
            }
        }

        public T LoadAsset<T>(string assetName) where T : UnityEngine.Object
        {
            assetName += GetExtension<T>();
            string setName;
            string bundleName;
            if (AssetBundleSetUtil.GetAssetOwnerInfo(assetName, out setName, out bundleName))
            {
                if (!string.IsNullOrEmpty(setName))
                {
                    var loader = GetOrCreateLoader(setName);
                    //return loader.Load<T>(bundleName, assetName);
                }
                else
                {
                    Debug.LogError("LoadAsset:setName get is empty.. assetName:" + assetName);
                }
            }
            else
            {
                Debug.LogError("LoadAsset:GetAssetOwnerInfo Error.. assetName:" +assetName);
            }

            return null;
        }

        public T LoadAndInstantiateAsset<T>(string assetName, string instantiateName = "") where T:UnityEngine.Object
        {
            assetName += GetExtension<T>();
            string setName;
            string bundleName;
            if(AssetBundleSetUtil.GetAssetOwnerInfo(assetName,out setName, out bundleName))
            {
                if (!string.IsNullOrEmpty(setName))
                {
                    var loader = GetOrCreateLoader(setName);
                    //return loader.LoadAndInstantiate<T>(bundleName, assetName, instantiateName);
                }
                else
                {
                    Debug.LogError("LoadAndInstantiateAsset:setName get is empty.. assetName:" + assetName);
                }
            }
            else
            {
                Debug.LogError("LoadAndInstantiateAsset:GetAssetOwnerInfo fail.. assetName:" + assetName);
            }

            return null;
        }

        /// <summary>
        /// 加载scene的assetbundle
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="onLoadedAssetbundle"></param>
        /// <param name="component"></param>
        public void LoadSceneAssetBundle(string scene,System.Action<AssetBundle> onLoadedAssetbundle)
        {
            scene = scene + ".unity";
            string setName;
            string bundleName;
            if (AssetBundleSetUtil.GetAssetOwnerInfo(scene, out setName, out bundleName))
            {
                LoadAssetBundleAsyn(bundleName, onLoadedAssetbundle);
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
    }

}
