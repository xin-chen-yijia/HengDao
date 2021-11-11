using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace HengDao
{
    public class SceneLoader
    {
        public static void Load(string scene, System.Action<float> updateHandle = null, System.Action completeHandle = null)
        {
            if (!string.IsNullOrEmpty(scene))
            {
                AsyncOperation opr = SceneManager.LoadSceneAsync(scene);
                CoroutineLauncher.current.StartCoroutine(Utils.WaitForAndDoLoop(() =>
                {
                    return new WaitForSeconds(0.1f);
                },
                () =>
                {
                    return !opr.isDone;
                },
                () =>
                {
                    if (updateHandle != null)
                    {
                        updateHandle(opr.progress);
                    }
                },
                ()=>
                {
                    if (completeHandle != null)
                    {
                        completeHandle();
                    }
                }));
            }
        }

        public static void LoadFromAB(string scene, System.Action onBeginLoadingHandle=null, System.Action<float> updateHandle = null, System.Action completeHandle = null)
        {
            SceneManager.LoadScene("Loading");
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
                    CoroutineLauncher.current.StartCoroutine(Utils.WaitForAndDo(opr,null));
                    CoroutineLauncher.current.StartCoroutine(Utils.WaitForAndDoLoop(() =>
                    {
                        return new WaitForEndOfFrame();
                    },
                    () =>
                    {
                        return !opr.isDone;
                    },
                    () =>
                    {
                        if (updateHandle != null)
                        {
                            updateHandle(opr.progress);
                        }
                    },
                    ()=>
                    {
                        if(completeHandle != null)
                        {
                            completeHandle();
                        }
                    }));
                });
            }
        }
    }
}
