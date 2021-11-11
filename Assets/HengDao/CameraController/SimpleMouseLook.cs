using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HengDao
{
    public class SimpleMouseLook : MonoBehaviour
    {

        private EMouseLook m_mouseLook;

        // Use this for initialization
        void Start()
        {
            m_mouseLook = new EMouseLook();
            m_mouseLook.Init(transform.parent, transform);
            m_mouseLook.SetCursorLock(false);
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetMouseButton(0))
            {
                m_mouseLook.LookRotation(transform.parent, transform);
            }
        }
    }

}

