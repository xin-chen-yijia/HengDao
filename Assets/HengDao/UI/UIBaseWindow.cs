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

        protected Transform rootTrans_;
        public RectTransform rootTrans
        {
            get
            {
                return rootTrans_ as RectTransform;
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

        public void SetRootTrans(Transform root)
        {
            rootTrans_ = root;
        }

        public virtual void Init()
        {
        }

        public virtual bool isShow
        {
            get
            {
                return rootTrans_.gameObject.activeSelf;
            }
        }

        public virtual void Show()
        {
            rootTrans_.gameObject.SetActive(true);
        }

        public virtual void Hide()
        {
            rootTrans_.gameObject.SetActive(false);
        }

        public virtual void SetPosition(Vector3 position)
        {
            rootTrans_.localPosition = position;
        }
    }
}

