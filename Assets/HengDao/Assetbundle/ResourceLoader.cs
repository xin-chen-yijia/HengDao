using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace HengDao
{

    public class ResourceLoader : IAssetLoader
    {
        private Dictionary<string, string> cachePaths_ = new Dictionary<string, string>();
        public T Load<T>(string assetName) where T : Object
        {
            if(cachePaths_.Count == 0)
            {
                //遍历
                string dir = Application.dataPath + "/Resources/";
                Queue<string> qu = new Queue<string>();
                qu.Enqueue(dir);
                while(qu.Count > 0)
                {
                    string d = qu.Dequeue();
                    string relativePath = d.Replace(dir, "");
                    if (relativePath.Length > 0)
                    {
                        relativePath += "/";
                    }
                    foreach (string item in Directory.GetFileSystemEntries(d))
                    {
                        string fileName = Path.GetFileName(item);
                        if ((File.GetAttributes(item) & FileAttributes.Directory) == FileAttributes.Directory)
                        {
                            qu.Enqueue(item.Replace("\\","/"));
                        }
                        else
                        {
                            if (fileName.EndsWith("prefab")) // !fileName.EndsWith("meta"))
                            {
                                string name_no_suf = fileName.Substring(0, fileName.LastIndexOf("."));
                                cachePaths_.Add(name_no_suf, relativePath + name_no_suf);
                            }
                        }
                    }
                }

            }

            if(cachePaths_.ContainsKey(assetName))
            {
                return Resources.Load<T>(cachePaths_[assetName]);
            }

            return null;
        }

        public void LoadAsync<T>(string assetName, System.Action<T> onLoaded) where T : UnityEngine.Object
        {
            ResourceRequest aysnOp = Resources.LoadAsync<T>(assetName);
            CoroutineLauncher.current.StartCoroutine(Utils.WaitForAndDo(aysnOp, ()=>
            {
                onLoaded(aysnOp.asset as T);
            }));
        }

        public T LoadAndInstantiate<T>(string assetName, string instantiateName = "", Transform parent = null) where T : Object
        {
            T t = Load<T>(assetName);
            if(t)
            {
                T nt = GameObject.Instantiate<T>(t,parent);
                nt.name = instantiateName;

                return nt;
            }

            return null;
        }
    }

}
