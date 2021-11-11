using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace HengDao
{
    public enum CanvasType
    {
        Overlay=0,
        Screen,
        World
    }
    public class UIBaseWindow
    {

        protected Transform mRootTrans;
        public RectTransform rootTrans
        {
            get
            {
                return mRootTrans as RectTransform;
            }
        }

        public virtual string prefab
        {
            get
            {
                return GetType().Name;
            }
        }

        public UIBaseWindow()
        {

        }

        //public bool LoadPrefab()
        //{
        //    //从AB包加载
        //    GameObject obj = ServiceLocator.GetABLoader().LoadAndInstantiate<GameObject>(prefab, prefab);
        //    if (obj != null)
        //    {
        //        mRootTrans = obj.transform;
        //        return true;
        //    }

        //    return false;
        //}

        public virtual void Init(Transform root)
        {
            mRootTrans = root;
        }

        public virtual bool isShow
        {
            get
            {
                return mRootTrans.gameObject.activeSelf;
            }
        }

        public virtual void Show()
        {
            mRootTrans.gameObject.SetActive(true);
        }

        public virtual void Hide()
        {
            mRootTrans.gameObject.SetActive(false);
        }

        public virtual void SetPosition(Vector3 position, CanvasType type=CanvasType.Overlay)
        {
            switch(type)
            {
                case CanvasType.Overlay:
                    mRootTrans.localPosition = position;
                    break;
                case CanvasType.Screen:
                    mRootTrans.localPosition = position;
                    break;
                case CanvasType.World:
                    mRootTrans.position = position;
                    break;
                default:
                    break;
            }
        }
    }
}

