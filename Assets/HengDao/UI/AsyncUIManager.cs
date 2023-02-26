using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace HengDao
{
    /// <summary>
    /// UI统一管理，使用异步加载资源的方式
    /// </summary>
    public class AsyncUIManager : MonoBehaviour
    {
        private Dictionary<string, UIBasePage> windows_ = new Dictionary<string, UIBasePage>();

#if UI_RESOURCE_LOADER
        private ResourceLoader assetLoader_ = new ResourceLoader();
#else
        private AsyncAssetBundleLoader assetLoader_ = null;
#endif

        private GameObject canvasRoot_ = null;

        public static async Task<AsyncUIManager> Create(AsyncAssetBundleLoader loader, string canvasName = "Canvas")
        {
            var tmpCanvas = await loader.LoadAssetAsync<GameObject>(canvasName);
            Debug.Assert(tmpCanvas);
            GameObject uiParent = GameObject.Instantiate<GameObject>(tmpCanvas);
            uiParent.transform.position = Vector3.zero;
            AsyncUIManager tmp = uiParent.AddComponent<AsyncUIManager>();
            tmp.canvasRoot_ = uiParent;
#if !UI_RESOURCE_LOADER
            tmp.assetLoader_ = loader;
#endif

            if (!GameObject.Find("EventSystem"))
            {
                Debug.LogError("Can't find EventSystem. UI maybe can't interact");
            }
            return tmp;
        }

        public T GetWindow<T>() where T : UIBasePage
        {
            // put window under canvas, no need instantiate
            string wndName = typeof(T).Name;
            if (!windows_.ContainsKey(wndName))
            {
                Transform wnd = canvasRoot_.transform.Find(wndName);
                Debug.Assert(wnd);
                var tmp = wnd.gameObject.AddComponent<T>();
                windows_[wndName] = tmp;
            }

            return windows_[wndName] as T;
        }
    }
}

