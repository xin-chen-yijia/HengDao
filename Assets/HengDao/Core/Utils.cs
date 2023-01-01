using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using UnityEngine;

namespace HengDao
{
    public static class Utils
    {
        public static Transform FindRecursive(this Transform transform,string child)
        {
            if(transform.name == child)
            {
                return transform;
            }
            foreach(Transform t in transform)
            {
                Transform res = FindRecursive(t,child);
                if(res)
                {
                    return res; 
                }
            }

            return null;
        }

        public static void DontDestroyOnLoad(GameObject obj)
        {
            Transform t = obj.transform;
            while(t.parent)
            {
                t = t.parent;
            }

            GameObject.DontDestroyOnLoad(t.gameObject);
        }

        public static void SetLayerRecursively(this GameObject obj,int layer)
        {
            if(obj == null)
            {
                return;
            }
            obj.layer = layer;
            foreach(Transform t in obj.transform)
            {
                SetLayerRecursively(t.gameObject,layer);
            }
        }

        public static void SetColliderEnableRecursively(this GameObject obj, bool enable)
        {
            if (obj == null)
            {
                return;
            }
            if(obj.GetComponent<Collider>())
            {
                obj.GetComponent<Collider>().enabled = enable;
            }
            
            foreach (Transform t in obj.transform)
            {
                SetColliderEnableRecursively(t.gameObject, enable);
            }
        }

        public static Texture2D ConvertFromBase64(string base64)
        {
            //ThreadPool.SetMaxThreads(4, 4);
            //ThreadPool.QueueUserWorkItem(new WaitCallback((object obj) =>
            //{
            //    try
            //    {
            //        //Texture2D tex = obj as Texture2D;
            //        byte[] bytes = Convert.FromBase64String(base64);
            //        texture.LoadImage(bytes);
            //    }
            //    catch(Exception e)
            //    {
            //        Debug.Log(e.Message);
            //    }
            //}),null);
            try
            {
                byte[] bytes = Convert.FromBase64String(base64);

                //MemoryStream memStream = new MemoryStream(bytes);
                //BinaryFormatter binFormatter = new BinaryFormatter();
                //Image img = (Image)binFormatter.Deserialize(memStream);

                //bitmap = new System.Drawing.Bitmap(ms);//将MemoryStream对象转换成Bitmap对象
                
                Texture2D tex = new Texture2D(640, 360);
                //tex.LoadRawTextureData(bytes);
                tex.LoadImage(bytes);
                return tex;
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                return null;
            }
        }
    }
}

