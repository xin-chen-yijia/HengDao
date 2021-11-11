using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using Newtonsoft.Json;

using HengDao;

public class AssetbundleEditor {

    [MenuItem("Tool/HengDao/build AB")]
    public static void BuildAssetBundles()
    {
        string cache = PlayerPrefs.GetString("ABPath", string.Empty);
        cache = string.IsNullOrEmpty(cache) ? "./" : cache;
        string path = EditorUtility.OpenFolderPanel("Build Assetbundle", cache, "");
        if (!string.IsNullOrEmpty(path))
        {
            ExcuteBuildAssetbundls(path);
        }
    }

    [MenuItem("Tool/HengDao/Clear and build AB")]
    public static void ClearAssetbundles()
    {
        string cache = PlayerPrefs.GetString("ABPath", string.Empty);
        cache = string.IsNullOrEmpty(cache) ? "./" : cache;
        string path = EditorUtility.OpenFolderPanel("Build Assetbundle", cache, "");
        if (!string.IsNullOrEmpty(path))
        {
            FileInfo fi = new FileInfo(path + "/sceneconfig.xml");
            if (fi.Exists)
            {
                fi.Delete();
            }
            DirectoryInfo dir = new DirectoryInfo(path);
            foreach (var v in dir.GetDirectories())
            {
                if (v.Name == "Resources")
                {
                    v.Delete(true);
                    return;
                }
            }

            ExcuteBuildAssetbundls(path);
        }
    }

    /// <summary>
    /// 执行打包
    /// </summary>
    /// <param name="path"></param>
    private static void ExcuteBuildAssetbundls(string path)
    {
        PlayerPrefs.SetString("ABPath", path);
        string abPath = path;
        if (!Directory.Exists(abPath))
        {
            Directory.CreateDirectory(abPath);
        }
        Debug.Log("start build...");
        BuildPipeline.BuildAssetBundles(abPath, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);
        Debug.Log("writing AB Info...");
        SaveAssetBundleCfg(path + "/../assetbundlesconfig.json", abPath);
        Debug.Log("build complete...");
    }

    private static string[] GetFileNames(string path,string pattern)
    {
        DirectoryInfo dir = new DirectoryInfo(path);
        if(dir.Exists)
        {
            FileInfo[] files = dir.GetFiles(pattern);
            string[] temp = new string[files.Length];
            for (int i = 0; i < files.Length; ++i)
            {
                AssetImporter ai = AssetImporter.GetAtPath("Assets/BeBuild/UI/EntryPage.prefab");
                string a = ai.assetBundleName;
                temp[i] = files[i].Name;//.Substring(0, files[i].Name.LastIndexOf('.'));
            }
            return temp;
        }

        return null;
    }

    /// <summary>
    /// 根据目录获取UI和Scene的assetbundle
    /// </summary>
    /// <param name="path"></param>
    /// <param name="pattern"></param>
    /// <returns></returns>
    private static ABJsonConfig.AssetBundleInfo[] GetAllABInfo()
    {
        string[] assetBundleNames = AssetDatabase.GetAllAssetBundleNames();
        ABJsonConfig.AssetBundleInfo[] infos = new ABJsonConfig.AssetBundleInfo[assetBundleNames.Length];
        for (int i = 0; i < assetBundleNames.Length; i++)
        {
            string assetBundleName = assetBundleNames[i];
            infos[i] = new ABJsonConfig.AssetBundleInfo();
            infos[i].name = assetBundleName;
            string[] aFiles = AssetDatabase.GetAssetPathsFromAssetBundle(assetBundleName);
            infos[i].elements = new string[aFiles.Length];
            for(int j = 0;j<aFiles.Length;++j)
            {
                Debug.Log(aFiles[j]);
                string p = aFiles[j];
                p = p.Substring(p.LastIndexOf('/') + 1);
                infos[i].elements[j] = p;
            }
        }

        return infos;
    }

    private static string[] GetAllABScenesName()
    {
        List<string> scenes = new List<string>();
        foreach(var v in AssetDatabase.GetAllAssetBundleNames())
        {
            foreach(var s in AssetDatabase.GetAssetPathsFromAssetBundle(v))
            {
                if(s.EndsWith("unity"))
                {
                    Debug.Log("scene:" + s);
                    string temp = s.Substring(s.LastIndexOf('/') + 1);
                    temp = temp.Substring(0, temp.LastIndexOf('.'));
                    scenes.Add(temp);
                }
            }
        }

        return scenes.ToArray();
    }
    private static void SaveAssetBundleCfg(string cfgPath, string abPath)
    {
        abPath = abPath.Replace('\\', '/');
        string setName = abPath.Substring(abPath.LastIndexOf('/') + 1);

        ABJsonConfig cfg;
        if (File.Exists(cfgPath))
        {
            JsonSerializer deserializer = new JsonSerializer();
            using (StreamReader sr = new StreamReader(cfgPath))
            {
                using (JsonReader reader = new JsonTextReader(sr))
                {
                    cfg = deserializer.Deserialize<ABJsonConfig>(reader);
                }
            }
        }
        else
        {
            cfg = new ABJsonConfig();
        }

        //把新元素添加到数组中，如果数组中同名的就替换，没有就添加到末尾
        System.Func<ABJsonConfig.AssetBundleSet[], ABJsonConfig.AssetBundleSet,ABJsonConfig.AssetBundleSet[]> appendBundleSet = 
            (setArray,newSet)=>
            {
                bool have = false;
                int len = setArray == null ? 0 : setArray.Length;
                for (int i = 0; i < len; ++i)
                {
                    if (setArray[i].name == newSet.name)
                    {
                        have = true;
                        setArray[i] = newSet;
                        break;
                    }
                }

                if (!have)
                {
                    //添加到末尾
                    ABJsonConfig.AssetBundleSet[] temp = new ABJsonConfig.AssetBundleSet[len + 1];
                    if (len > 0)
                    {
                        setArray.CopyTo(temp, 0);
                    }

                    temp[len] = newSet;
                    setArray = temp;
                }

                return setArray;
            };

        //all assetbundles
        ABJsonConfig.AssetBundleInfo[] uibundles = GetAllABInfo();
        if (uibundles != null)
        {
            ABJsonConfig.AssetBundleSet newSet = new ABJsonConfig.AssetBundleSet();
            newSet.name = setName;
            newSet.bundles = uibundles;

            cfg.sets = appendBundleSet(cfg.sets, newSet);
        }

        //scene config
        string[] sceneNames = GetAllABScenesName();
        if (sceneNames != null)
        {
            HashSet<string> set = cfg.scenes == null ? new HashSet<string>() : new HashSet<string>(cfg.scenes);

            set.UnionWith(sceneNames);
            cfg.scenes = new string[set.Count];
            set.CopyTo(cfg.scenes);
        }

        //FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write);
        //StreamWriter writer = new StreamWriter(fs);
        //writer.Write(JsonConvert.SerializeObject(cfg));
        //writer.Flush();
        //fs.Close();

        JsonSerializer serializer = new JsonSerializer();
        serializer.NullValueHandling = NullValueHandling.Ignore;
        using (StreamWriter sw = new StreamWriter(cfgPath))
        {
            using (JsonWriter writer = new JsonTextWriter(sw)
            {
                Formatting = Formatting.Indented,
                Indentation = 4,
                IndentChar = ' '
            })
            { 
                serializer.Serialize(writer, cfg);
            }
        }
    }

    [MenuItem("Tool/HengDao/Reset Tag", false, 1900)]
    public static void ResetTag()
    {
        GameObject selected = Selection.activeObject as GameObject;
        if (selected == null)
        {
            return;
        }

        ResetSceneObjTag(selected.transform);
    }

    [MenuItem("Tool/HengDao/Reset Tag", true)]
    static bool ResetSceneObjTagValidate()
    {
        return Selection.activeGameObject != null;
    }

    /// <summary>
    /// 重置物体及其子物体的tag
    /// </summary>
    /// <param name="obj"></param>
    private static void ResetSceneObjTag(Transform obj)
    {
        obj.tag = "Untagged";
        for (int i = 0; i < obj.childCount; ++i)
        {
            ResetSceneObjTag(obj.GetChild(i));
        }
    }


    //private static readonly string[] tags =
    //{
    //    "Extinguisher",
    //    "Fire",
    //    "TipsUI",
    //    "FireAlarm",
    //    "Combustible",
    //    "BoxArea",
    //    "SpreadSmoke",
    //    "TeleportArea"
    //};
    //[MenuItem("Tool/Import Fire Tags",false,1900)]
    //private static void ImportFireTags()
    //{
    //    if (EditorUtility.DisplayDialog("Import Tags", "Import fire tags may change the objects tag setting before.Import now?", "import", "cancle"))
    //    {
    //        HengDao.TagManager.SetTags(tags);
    //    }
    //}

    [MenuItem("Tool/HengDao/Create BeBuild Folder", false, 2000)]
    private static void CreateBeBuildFolder()
    {
        if(!AssetDatabase.IsValidFolder("Assets/BeBuild"))
        {
            AssetDatabase.CreateFolder("Assets","BeBuild");
            AssetDatabase.CreateFolder("BeBuild", "Scenes");
        }
    }
}
