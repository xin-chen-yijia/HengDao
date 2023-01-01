using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HengDao
{
    public class Logger 
    {
#if UNITY_EDITOR
        public static void Info(object message)
        {
            Debug.Log(message);
        }

        public static void Warning(object message)
        {
            Debug.LogWarning(message);
        }

        public static void Error(object message)
        {
            Debug.LogError(message);
        }
#else
        public static void Info(object message)
        {
        }

        public static void Warning(object message)
        {
        }

        public static void Error(object message)
        {
        }
#endif
    }

}