using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace  HengDao
{
    public class CoroutineLauncher : MonoBehaviour
    {
        // // Start is called before the first frame update
        // void Start()
        // {
            
        // }

        // // Update is called once per frame
        // void Update()
        // {
            
        // }

        private static CoroutineLauncher current_ = null;
        public static CoroutineLauncher current
        {
            get
            {
                if(current_ == null)
                {
                    GameObject obj = new GameObject("CoroutineLauncher");
                    current_ = obj.AddComponent<CoroutineLauncher>();
                    DontDestroyOnLoad(obj);
                }

                return current_;
            }
        }

    }
}

