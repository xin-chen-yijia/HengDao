using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HengDao
{
    public interface IAssetLoader
    {
        T Load<T>(string assetName) where T : UnityEngine.Object;

        void LoadAsync<T>(string assetName, System.Action<T> onLoaded) where T : UnityEngine.Object;

        T LoadAndInstantiate<T>(string assetName, string instantiateName = "",Transform parent=null) where T : UnityEngine.Object;
    }
}
