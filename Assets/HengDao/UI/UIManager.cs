using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;

namespace HengDao
{
    public class UIManager
    {

        private static UIManager mInstance = new UIManager();
        public static UIManager GetInstance()
        {
            return mInstance;
        }

        private GameObject mCanvas = null;
        private GameObject mWorldCanvasObj = null;
        private GameObject mEventSystem = null;

        private bool overlayCanvasInited
        {
            get
            {
                return mCanvas != null;
            }
        }

        private bool worldCanvasInited
        {
            get
            {
                return mWorldCanvasObj != null;
            }
        }

        //2D ui
        private Dictionary<string, UIBaseWindow> windows_ = new Dictionary<string, UIBaseWindow>();

        //3d ui
        private Dictionary<int, GameObject> world_ui_objs_ = new Dictionary<int, GameObject>();
        public static Vector2 referenceResolution { get; set; } = new Vector2(9, 0);

        private UIManager()
        {
            
        }

        private void CreateWorldCanvas()
        {
            if(!mWorldCanvasObj)
            {
                mWorldCanvasObj = new GameObject("WorldCanvas");
                GameObject.DontDestroyOnLoad(mWorldCanvasObj);
                
                Canvas canvas = mWorldCanvasObj.AddComponent<Canvas>();
                mWorldCanvasObj.AddComponent<CanvasScaler>();
                mWorldCanvasObj.AddComponent<GraphicRaycaster>();

                canvas.worldCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
                canvas.sortingOrder = 1;
                RectTransform rectTrans = mWorldCanvasObj.GetComponent<RectTransform>();
                rectTrans.position = Vector3.zero;
                rectTrans.sizeDelta.Set(800, 600);
                float ss = 10.0f / 800.0f;
                rectTrans.localScale = new Vector3(ss, ss, ss);
            }


            if (GameObject.FindObjectOfType<EventSystem>() == null)
            {
                mEventSystem = new GameObject("EventSystem");
                mEventSystem.AddComponent<EventSystem>();
                mEventSystem.AddComponent<StandaloneInputModule>();

                GameObject.DontDestroyOnLoad(mEventSystem);
            }
        }

        private void CreateOverlayScreenCanvas()
        {
            if(!mCanvas)
            {
                mCanvas = new GameObject("OverlayScreenCanvas");
                GameObject.DontDestroyOnLoad(mCanvas);

                mCanvas.AddComponent<Canvas>();
                //mCanvas.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);   //not work
                mCanvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
                mCanvas.AddComponent<CanvasScaler>();
                mCanvas.AddComponent<GraphicRaycaster>();

                //TODO:设置分辨率
                //mCanvas.GetComponent<RectTransform>().sizeDelta = new Vector2(1920, 1080);
            }

            if (GameObject.FindObjectOfType<EventSystem>() == null)
            {
                mEventSystem = new GameObject("EventSystem");
                mEventSystem.AddComponent<EventSystem>();
                mEventSystem.AddComponent<StandaloneInputModule>();

                GameObject.DontDestroyOnLoad(mEventSystem);
            }           
        }

        public void SetScaleMode(CanvasScaler.ScaleMode mode)
        {
            CanvasScaler scaler = mCanvas.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = mode;
            switch (mode)
            {
                case CanvasScaler.ScaleMode.ScaleWithScreenSize:
                    scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                    scaler.referenceResolution = referenceResolution;
                    break;
                case CanvasScaler.ScaleMode.ConstantPixelSize:
                    scaler.scaleFactor = 1.0f;
                    scaler.referencePixelsPerUnit = 100;
                    break;
                case CanvasScaler.ScaleMode.ConstantPhysicalSize:
                    break;
            }
        }

        public T GetWindow<T>() where T : UIBaseWindow
        {
            if(!overlayCanvasInited)
            {
                CreateOverlayScreenCanvas();
            }
            
            string name = typeof(T).Name;
            UIBaseWindow wnd;
            windows_.TryGetValue(name, out wnd);
            if (wnd == null)
            {
                Type t = Type.GetType(name);
                if (t != null)
                {
                    wnd = Activator.CreateInstance(t) as UIBaseWindow;
                    GameObject obj = ServiceLocator.GetABLoader().LoadAndInstantiate<GameObject>(wnd.prefab, wnd.prefab, mCanvas.transform);
                    Debug.Assert(obj);
                    wnd.Init(obj.transform);
                    
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

        public GameObject CreateWordUI(string type, Vector3 position, Quaternion rotation)
        {
            if (!worldCanvasInited)
            {
                CreateWorldCanvas();
            }

            GameObject obj = ServiceLocator.GetABLoader().LoadAndInstantiate<GameObject>(type,type, mWorldCanvasObj.transform);
            if(obj)
            {
                //obj.transform.SetParent(mWorldCanvas.transform);
                obj.transform.localPosition = position;
                obj.transform.rotation = rotation;
                obj.GetComponent<RectTransform>().localScale = Vector3.one;
            }
            return obj;       
        }

        public T CreateWordUI<T>(string uiName, Vector3 position, Quaternion rotation,Vector3 scale,Space space=Space.Self) where T: UIBaseWindow
        {
            if (!worldCanvasInited)
            {
                CreateWorldCanvas();
            }

            string name = typeof(T).Name;
            Type t = Type.GetType(name);
            if (t != null)
            {
                UIBaseWindow wnd = Activator.CreateInstance(t) as UIBaseWindow;

                GameObject obj = ServiceLocator.GetABLoader().LoadAndInstantiate<GameObject>(wnd.prefab, wnd.prefab, mWorldCanvasObj.transform);
                if(space == Space.Self)
                {
                    obj.GetComponent<RectTransform>().localPosition = position;
                    obj.GetComponent<RectTransform>().localRotation = rotation;
                    obj.GetComponent<RectTransform>().localScale = scale;
                }
                else
                {
                    obj.GetComponent<RectTransform>().position = position;
                    obj.GetComponent<RectTransform>().rotation = rotation;
                    obj.GetComponent<RectTransform>().localScale = scale;
                }

                Debug.Assert(obj);
                wnd.Init(obj.transform);

                return wnd as T;
            }
            else
            {
                Logger.Error("No type " + name);
                return null;
            }
        }

        public void Release()
        {
            GameObject.Destroy(mCanvas);
            GameObject.Destroy(mEventSystem);

            windows_.Clear();
        }
    }
}
