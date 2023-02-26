using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;

namespace HengDao
{
    /// <summary>
    /// 统一管理UI
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        private Dictionary<string, UIBasePage> windows_ = new Dictionary<string, UIBasePage>();

#if UI_RESOURCE_LOADER
        private ResourceLoader assetLoader_ = new ResourceLoader();
#else
        private AssetBundleLoader assetLoader_ = null;
#endif

        private GameObject canvasRoot_ = null;

        void Start()
        {
            
        }

        public static UIManager Create(AssetBundleLoader loader,string canvasName = "Canvas")
        {
            GameObject uiParent = loader.LoadAssetAndInstantiate<GameObject>(canvasName);
            Debug.Assert(uiParent);
            UIManager tmp = uiParent.AddComponent<UIManager>();
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
            // old impl
            //Debug.Assert(assetLoader_ != null);
            //string name = typeof(T).Name;
            //UIBaseWindow wnd;
            //windows_.TryGetValue(name, out wnd);
            //if (wnd == null)
            //{
            //    Type t = Type.GetType(name);
            //    if (t != null)
            //    {
            //        wnd = Activator.CreateInstance(t) as UIBaseWindow;
            //        GameObject obj = assetLoader_.LoadAssetAndInstantiate<GameObject>(wnd.prefab, wnd.prefab, transform);
            //        Debug.Assert(obj);
            //        wnd.SetRootTrans(obj.transform);
            //        wnd.Init();

            //        windows_.Add(name, wnd);
            //        return wnd as T;
            //    }
            //    else
            //    {
            //        Logger.Error("No type " + name);
            //        return null;
            //    }
            //}
            //else
            //{
            //    return windows_[name] as T;
            //}

            // put window under canvas, no need instantiate
            string wndName = typeof(T).Name;
            if(!windows_.ContainsKey(wndName))
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
