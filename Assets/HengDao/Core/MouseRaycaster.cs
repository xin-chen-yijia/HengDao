using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace HengDao
{
    public class MouseRaycaster : MonoBehaviour
    {
        public Camera eventCamera;

        public class VoidDelegate : UnityEngine.Events.UnityEvent<GameObject> { }
        public class OneGameObjectDelegate : UnityEngine.Events.UnityEvent<GameObject> { }
        public OneGameObjectDelegate onClick { get; } = new OneGameObjectDelegate();
        public OneGameObjectDelegate onHover { get; } = new OneGameObjectDelegate();
        public OneGameObjectDelegate onExit { get; } = new OneGameObjectDelegate();

        //当前选择物体
        private GameObject curPointer = null;

        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
            RaycastHit hit;
            if (Physics.Raycast(eventCamera.ScreenPointToRay(Input.mousePosition), out hit, 2000))
            {
                if(curPointer != hit.collider.gameObject)
                {
                    onExit?.Invoke(curPointer);
                }
                curPointer = hit.collider.gameObject;
                onHover?.Invoke(curPointer);
                if (Input.GetMouseButtonDown(0))
                {
                    onClick?.Invoke(curPointer);
                }
            }
            else
            {
                onExit?.Invoke(curPointer);
                curPointer = null;
            }
        }
    }
}