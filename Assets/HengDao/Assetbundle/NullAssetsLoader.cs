using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HengDao
{
    public class NullAssetsLoader : IAssetLoader
    {
        public T Load<T>(string assetName) where T : UnityEngine.Object
        {
            return new GameObject() as T;
        }

        public void LoadAsync<T>(string assetName, System.Action<T> onLoaded) where T : UnityEngine.Object
        {
            if(onLoaded != null)
            {  
                onLoaded(new GameObject() as T);//不清楚为什么不能直接转换而需要一个as
            }
            
        }

        public T LoadAndInstantiate<T>(string assetName, string instantiateName = "", Transform parent = null) where T : UnityEngine.Object
        {
            return new GameObject() as T;
        }
    }
}
