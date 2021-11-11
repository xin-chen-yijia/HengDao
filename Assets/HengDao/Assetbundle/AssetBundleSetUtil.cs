using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace HengDao
{
    public class AssetBundleSetUtil
    {
        private static ABJsonConfig cfg_;

        private static string s_path;

        public static string assetbundlesDir
        {
            get
            {
                return s_path.Substring(0, s_path.LastIndexOf('/'));
            }
        }

        public static void InitConfigFile(string path)
        {
            s_path = path.Replace("\\", "/");
            JsonSerializer serializer = new JsonSerializer();
            serializer.NullValueHandling = NullValueHandling.Ignore;

            using (StreamReader sr = new StreamReader(path))
            {
                using (JsonReader reader = new JsonTextReader(sr))
                {
                    cfg_ = serializer.Deserialize<ABJsonConfig>(reader);
                }
            }
        }

        public static string[] GetScenes()
        {
            return cfg_.scenes.Clone() as string[];
        }

        /// <summary>
        /// 根据assetbundle和assetName获取在那个集合里
        /// </summary>
        /// <param name="bundleName"></param>
        /// <returns></returns>
        public static string GetSetNameWithBundle(string bundleName)
        {
            foreach(var v in cfg_.sets)
            {
                foreach(var s in v.bundles)
                {
                    if(s.name == bundleName)
                    {
                         return v.name;
                    }
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// 根据asset获取assetbundle的集合
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public static bool GetAssetOwnerInfo(string assetName, out string setName, out string bundleName)
        {
            foreach (var v in cfg_.sets)
            {
                foreach (var s in v.bundles)
                {
                    foreach(var e in s.elements)
                    {
                        if (e == assetName)
                        {
                            setName = v.name;
                            bundleName = s.name;
                            return true;
                        }
                    }
                }
            }

            setName = string.Empty;
            bundleName = string.Empty;
            return false;
        }
    }
}


