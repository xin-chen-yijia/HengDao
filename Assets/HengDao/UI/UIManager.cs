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

        private static UIManager instance_ = null;
        public static UIManager GetInstance()
        {
            return instance_;
        }

        private Dictionary<string, UIBaseWindow> windows_ = new Dictionary<string, UIBaseWindow>();

#if UI_RESOURCE_LOADER
        private ResourceLoader assetLoader_ = new ResourceLoader();
#else
        private AssetBundleLoader assetLoader_ = null;
#endif

        void Awake()
        {
            instance_ = this;
            DontDestroyOnLoad();
        }

        void Start()
        {
            
        }

        public void SetLoader(AssetBundleLoader loader)
        {
#if !UI_RESOURCE_LOADER
            assetLoader_ = loader;
#endif
        }

        public void DontDestroyOnLoad()
        {
            DontDestroyOnLoad(gameObject);
            var eventObj = GameObject.Find("EventSystem");
            if(eventObj)
            {
                DontDestroyOnLoad(eventObj);
            }
            
        }

        public T GetWindow<T>() where T : UIBaseWindow
        {
            Debug.Assert(assetLoader_ != null);
            string name = typeof(T).Name;
            UIBaseWindow wnd;
            windows_.TryGetValue(name, out wnd);
            if (wnd == null)
            {
                Type t = Type.GetType(name);
                if (t != null)
                {
                    wnd = Activator.CreateInstance(t) as UIBaseWindow;
                    GameObject obj = assetLoader_.LoadAndInstantiate<GameObject>(wnd.prefab, wnd.prefab, transform);
                    Debug.Assert(obj);
                    wnd.SetRootTrans(obj.transform);
                    wnd.Init();
                    
                    windows_.Add(name, wnd);
                    return wnd as T;
                }
                else
                {
                    Logger.Error("No type " + name);
                    return null;
                }
            }
            else
            {
                return windows_[name] as T;
            }
        }
    }
}
