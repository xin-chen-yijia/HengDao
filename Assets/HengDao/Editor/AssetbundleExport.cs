using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

using HengDao;

public class AssetbundleExport {

    const string kAssetbundlesBuildStr = "AssetbundlesBuild";
    const string kHengDaoABPathStr = "HengDaoABPath";

    /// <summary>
    /// 构建Assetbundle
    /// 
    /// 把需要构建assetbundle的素材统一放入AssetbundleBuild下
    /// AssetbundleBuild
    /// ---每个scene单独一个文件夹
    /// ---Dynamic
    /// ---Common
    /// ---其它文件夹
    /// </summary>
    [MenuItem("Tool/HengDao/build AB")]
    public static void BuildAssetBundles()
    {
        string cache = PlayerPrefs.GetString(kHengDaoABPathStr, string.Empty);
        cache = string.IsNullOrEmpty(cache) ? "./" : cache;
        string path = EditorUtility.OpenFolderPanel("Build Assetbundle", cache, "");
        if (!string.IsNullOrEmpty(path))
        {
            PlayerPrefs.SetString(kHengDaoABPathStr, path);
            ExcuteBuildAssetbundls(path);
        }
    }

    /// <summary>
    /// name assetbundles
    /// </summary>
    /// <param name="prefix"></param>
    private static void AssignAssetbundleNames(string prefix="")
    {
        /// AssetbundlesBuild
        /// ---每个scene单独一个文件夹
        /// ---Dynamic
        /// ---Common
        /// ---其它文件夹
        string AssetbundlesBuildDir = Path.Combine(Application.dataPath, kAssetbundlesBuildStr);
        DirectoryInfo buildFolder = new DirectoryInfo(AssetbundlesBuildDir);
        foreach(var v in buildFolder.GetDirectories())
        {
            AssignAssetBundleNameInFolder("AssetbundlesBuild/"+v.Name, prefix + v.Name, AssetBundlePresets.kVariantName);
        }
    }

    /// <summary>
    /// name assetbundles in specified folder
    /// </summary>
    /// <param name="relativePath"></param>
    /// <param name="assetBundleName"></param>
    /// <param name="variantName"></param>
    private static void AssignAssetBundleNameInFolder(string relativePath,string assetBundleName, string variantName)
    {
        DirectoryInfo folderInfo = new DirectoryInfo(Path.Combine(Application.dataPath, relativePath));
        foreach (var v in folderInfo.GetFiles())
        {
            if (v.Name.EndsWith(".meta"))
            {
                continue;
            }

            AssetImporter ai = AssetImporter.GetAtPath("Assets/" + relativePath + "/" + v.Name);
            ai.SetAssetBundleNameAndVariant(assetBundleName, variantName);
        }

        foreach (var v in folderInfo.GetDirectories())
        {
            AssignAssetBundleNameInFolder(relativePath + "/" + v.Name, assetBundleName, variantName);
        }
    }

    private static void WriteBuildInfoToFile(string path)
    {
        AssetBundleBuildInfo buildInfo = new AssetBundleBuildInfo();
        buildInfo.version = Application.unityVersion;
        string jsonString = JsonUtility.ToJson(buildInfo);
        File.WriteAllText(Path.Combine(path, AssetBundlePresets.kBuildInfoFileName), jsonString);
    }

    [MenuItem("Tool/HengDao/Clear and build AB")]
    public static void ClearAssetbundles()
    {
        string path = PlayerPrefs.GetString(kHengDaoABPathStr, string.Empty);
        if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
        {
            DirectoryInfo dir = new DirectoryInfo(path);
            dir.Delete(true);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
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
        Debug.Log("=====Assign assetbundle names======");
        AssignAssetbundleNames(AssetBundleLoader.ParseABNamePrefixFromPath(path).ToLower());

        Debug.Log("=====start build======");
        BuildPipeline.BuildAssetBundles(path, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);
        WriteBuildInfoToFile(path);

        Debug.Log("=====build complet=====");
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
                    string temp = s.Substring(s.LastIndexOf('/') + 1);
                    temp = temp.Substring(0, temp.LastIndexOf('.'));
                    scenes.Add(temp);
                }
            }
        }

        return scenes.ToArray();
    }

    [MenuItem("Tool/HengDao/Reset AssetBundle Name")]
    public static void ResetAssetBundleNames()
    {
        string[] assetBundleNames = AssetDatabase.GetAllAssetBundleNames();
        for (int i = 0; i < assetBundleNames.Length; i++)
        {
            string assetBundleName = assetBundleNames[i];
            string[] aFiles = AssetDatabase.GetAssetPathsFromAssetBundle(assetBundleName);
            for (int j = 0; j < aFiles.Length; ++j)
            {
                AssetImporter ai = AssetImporter.GetAtPath(aFiles[j]);
                ai.SetAssetBundleNameAndVariant("", "");
            }
        }
    }

    [MenuItem("Tool/HengDao/Build AssetBundles Folder", priority = 2000)]
    public static void BuildAssetBundleFolders()
    {
        if (!Directory.Exists(Path.Combine(Application.dataPath, kAssetbundlesBuildStr)))
        {
            AssetDatabase.CreateFolder("Assets", kAssetbundlesBuildStr);
            AssetDatabase.CreateFolder("Assets/" + kAssetbundlesBuildStr, AssetBundlePresets.kAssetBundleName);
        }
    }
}
