using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System.Linq;
using System.IO;

public class AssetbundleExportWindow : EditorWindow
{
    const string kHengDaoABPathStr = "HengDaoABPath";
    const string kBuildAssetBundleOptionStr = "BuildAssetBundleOption";
    const string kBuildTargetStr = "BuildTarget";
    const string kPluginsPlayerPrefStr = "Plugins";
    const string kClearAssetBundlePrefStr = "ClearAB";

    [MenuItem("Window/HengDao/AssetBundle Export")]
    public static void ShowMyEditor()
    {
        // This method is called when the user selects the menu item in the Editor
        EditorWindow wnd = GetWindow<AssetbundleExportWindow>();
        wnd.titleContent = new GUIContent("AssetBundleExport");

        // Limit size of the window
        wnd.minSize = new Vector2(450, 200);
        wnd.maxSize = new Vector2(1920, 720);
    }

    private class AssetBundleBuildPath
    {
        public string path;
        public bool willBuild;
    }

    public void CreateGUI()
    {
        List<string> excludeFolders = new List<string>()
        {
            "Editor",
            "Resources",
            "Scenes",
            "HengDao"
        };

        List<AssetBundleBuildPath> pathsToBuild = new List<AssetBundleBuildPath>();

        // 要打包的文件夹
        DirectoryInfo dirInfo = new DirectoryInfo(Application.dataPath);
        foreach (var v in dirInfo.GetDirectories())
        {
            var elm = new Toggle(v.Name);
            if(excludeFolders.Contains(v.Name))
            {
                continue;
            }

            AssetBundleBuildPath mark = new AssetBundleBuildPath();
            mark.path = v.Name;
            mark.willBuild = false;

            pathsToBuild.Add(mark);
            elm.RegisterValueChangedCallback((val) =>
            {
                mark.willBuild = val.newValue;
            });
            rootVisualElement.Add(elm);
        }

        // 构建选项
        string lastBuildOption = PlayerPrefs.GetString(kBuildAssetBundleOptionStr, BuildAssetBundleOptions.None.ToString());
        string[] buildAssetBundleOps = System.Enum.GetNames(typeof(BuildAssetBundleOptions));
        DropdownField opsDropdownField = new DropdownField("build options", new List<string>(buildAssetBundleOps), lastBuildOption);
        rootVisualElement.Add(opsDropdownField);

        // 目标平台选项
        string lastBuildTarget = PlayerPrefs.GetString(kBuildTargetStr, BuildTarget.StandaloneWindows.ToString());
        string[] targets = System.Enum.GetNames(typeof(BuildTarget));
        DropdownField targetDropdownField = new DropdownField("build target", new List<string>(targets), lastBuildTarget);
        rootVisualElement.Add(targetDropdownField);

        // 插件列表
        TextField pluginsFiled = new TextField("plugins");
        string pluginsVal = PlayerPrefs.GetString(kPluginsPlayerPrefStr,string.Empty);
        pluginsFiled.value = pluginsVal;
        rootVisualElement.Add(pluginsFiled);

        string cache = PlayerPrefs.GetString(kHengDaoABPathStr, string.Empty);
        TextField outputFiled = new TextField("output:");
        outputFiled.value = cache;
        Button browserBtn = new Button(() =>
        {
            string path = EditorUtility.OpenFolderPanel("Build Assetbundle", cache, "");
            outputFiled.value = path;
        });
        browserBtn.text = "browser...";

        rootVisualElement.Add(outputFiled);
        rootVisualElement.Add(browserBtn);

        var clearABToggle = new Toggle("clean old assetbundle");
        clearABToggle.value = PlayerPrefs.GetInt(kClearAssetBundlePrefStr,0) == 0;
        rootVisualElement.Add(clearABToggle);
            
        // 构建Assetbundle
        var buildBtn = new Button(() =>
        {
            string outputPath = outputFiled.value;

            PlayerPrefs.SetInt(kClearAssetBundlePrefStr, clearABToggle.value ? 1 : 0);
            PlayerPrefs.SetString(kHengDaoABPathStr, outputPath);
            PlayerPrefs.SetString(kBuildAssetBundleOptionStr, opsDropdownField.value);
            PlayerPrefs.SetString(kBuildTargetStr, targetDropdownField.value);
            PlayerPrefs.SetString(kPluginsPlayerPrefStr, pluginsFiled.value);

            // 设置AB包名字
            Debug.Log("=====Assign assetbundle names======");
            foreach (var v in pathsToBuild)
            {
                if(v.willBuild)
                {
                    AssetbundleExport.AssignAssetBundleNameInFolder(v.path, v.path, HengDao.AssetBundlePresets.kPandaVariantName);
                }
            }

            if (!string.IsNullOrEmpty(outputPath))
            {
                if (clearABToggle.value)
                {
                    AssetbundleExport.ClearAndBuildAssetbundles(outputPath);
                }

                // 执行构建
                Debug.Log("=====start build======");
                AssetbundleExport.plugins = new List<string>(pluginsFiled.value.Split(";"));
                AssetbundleExport.ExcuteBuildAssetbundls(outputPath, System.Enum.Parse<BuildAssetBundleOptions>(opsDropdownField.value),System.Enum.Parse<BuildTarget>(targetDropdownField.value));
                Debug.Log("=====build complet=====");

                Close();
            }
        });
        buildBtn.text = "build";
        rootVisualElement.Add(buildBtn);

        var resetNameBtn = new Button(() => {
            AssetbundleExport.ResetAssetBundleNames();
        });
        resetNameBtn.text = "rest assetbundle name";

        rootVisualElement.Add(resetNameBtn);

        //// A TwoPaneSplitView always needs exactly two child elements
        //var leftPane = new ListView();
        //splitView.Add(leftPane);
        //m_RightPane = new ScrollView(ScrollViewMode.VerticalAndHorizontal);
        //splitView.Add(m_RightPane);

        //// Initialize the list view with all sprites' names
        //leftPane.makeItem = () => new Label();
        //leftPane.bindItem = (item, index) => { (item as Label).text = allObjects[index].name; };
        //leftPane.itemsSource = allObjects;

        // React to the user's selection
        //leftPane.onSelectionChange += OnSpriteSelectionChange;
    }

    //private void OnSpriteSelectionChange(IEnumerable<object> selectedItems)
    //{
    //    // Clear all previous content from the pane
    //    m_RightPane.Clear();

    //    // Get the selected sprite
    //    var selectedSprite = selectedItems.First() as Sprite;
    //    if (selectedSprite == null)
    //        return;

    //    // Add a new Image control and display the sprite
    //    var spriteImage = new Image();
    //    spriteImage.scaleMode = ScaleMode.ScaleToFit;
    //    spriteImage.sprite = selectedSprite;

    //    // Add the Image control to the right-hand pane
    //    m_RightPane.Add(spriteImage);
    //}
}
