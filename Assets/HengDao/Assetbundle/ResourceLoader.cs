using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace HengDao
{

    public class ResourceLoader
    {
        private Dictionary<string, string> cachePaths_ = new Dictionary<string, string>();
        public T Load<T>(string assetName) where T : Object
        {
            return Resources.Load<T>(assetName);
        }

        public void LoadAsync<T>(string assetName, System.Action<T> onLoaded) where T : UnityEngine.Object
        {
            ResourceRequest aysnOp = Resources.LoadAsync<T>(assetName);
            CoroutineLauncher.current.StartCoroutine(WaitLoadAsset(aysnOp, onLoaded));
        }

        IEnumerator WaitLoadAsset<T>(ResourceRequest request, System.Action<T> onLoaded) where T : UnityEngine.Object
        {
            yield return request;

            onLoaded(request.asset as T);
        }

        public T LoadAndInstantiate<T>(string assetName, string instantiateName = "", Transform parent = null) where T : Object
        {
            T t = Load<T>(assetName);
            if(t)
            {
                T nt = GameObject.Instantiate<T>(t,parent);
                nt.name = instantiateName;

                return nt;
            }

            return null;
        }
    }

}
