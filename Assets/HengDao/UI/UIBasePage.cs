using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace HengDao
{
    public class UIBasePage : MonoBehaviour
    {
        public virtual bool isShow
        {
            get
            {
                return gameObject.activeSelf;
            }
        }

        public virtual void Show()
        {
            gameObject.SetActive(true);
        }

        public virtual void Hide()
        {
            gameObject.SetActive(false);
        }

        public virtual void SetPosition(Vector3 position)
        {
            transform.localPosition = position;
        }
    }
}

