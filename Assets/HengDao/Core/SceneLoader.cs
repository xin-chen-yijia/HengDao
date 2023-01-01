using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace HengDao
{
    public class SceneLoader
    {
        static IEnumerator UpdateLoadSceneStatus(AsyncOperation opr, System.Action<float> updateHandle, System.Action completeHandle)
        {
            while (!opr.isDone)
            {
                yield return new WaitForSeconds(0.1f);

                if (updateHandle != null)
                {
                    updateHandle(opr.progress);
                }
            }

            if (completeHandle != null)
            {
                completeHandle();
            }
        }

        /// <summary>
        /// load scene in build setting scene list
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="updateHandle"></param>
        /// <param name="completeHandle"></param>
        public static void Load(string scene, System.Action<float> updateHandle = null, System.Action completeHandle = null)
        {
            if (!string.IsNullOrEmpty(scene))
            {
                AsyncOperation opr = SceneManager.LoadSceneAsync(scene);
                CoroutineLauncher.current.StartCoroutine(UpdateLoadSceneStatus(opr,updateHandle,completeHandle));
            }
        }

    
        /// <summary>
        /// load scene from assetbundles
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="onBeginLoadingHandle"></param>
        /// <param name="updateHandle"></param>
        /// <param name="completeHandle"></param>
        public static void LoadFromAB(string scene, System.Action onBeginLoadingHandle=null, System.Action<float> updateHandle = null, System.Action completeHandle = null)
        {
            if(onBeginLoadingHandle!=null)
            {
                onBeginLoadingHandle();
            }

            if (!string.IsNullOrEmpty(scene))
            {
                AssetBundleLoader loader = ServiceLocator.GetABLoader() as AssetBundleLoader;
                string bundleName, setName;
                AssetBundleSetUtil.GetAssetOwnerInfo(scene + ".unity",out setName,out bundleName);
                loader.LoadAssetBundleAsyn(bundleName, (v) =>
                {
                    AsyncOperation opr = SceneManager.LoadSceneAsync(scene);
                    CoroutineLauncher.current.StartCoroutine(UpdateLoadSceneStatus(opr,updateHandle,completeHandle));
                });
            }
        }
    }
}
