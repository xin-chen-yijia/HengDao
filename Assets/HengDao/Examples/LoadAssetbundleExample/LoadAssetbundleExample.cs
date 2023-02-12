using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using HengDao;
using System.Threading.Tasks;
using UnityEngine.UIElements;

public class LoadAssetbundleExample : MonoBehaviour
{
    public UIDocument uiDoc;

    private AssetBundleLoader loader_ = null;

    // Start is called before the first frame update
    async void Start()
    {
        //loader.Init("C:/Temp/test"); // assetbundles folder
        //loader.Init("http://127.0.0.1:3333/test"); // assetbundles folder
        //loader.Init("file:///C:/Temp/test"); // assetbundles folder

        TextField pathLabel = uiDoc.rootVisualElement.Q<TextField>("AssetbundlePath");
        TextField objectLabel = uiDoc.rootVisualElement.Q<TextField>("ObjectPath");
        Toggle isSceneToggle = uiDoc.rootVisualElement.Q<Toggle>("IsScene");

        VisualElement loadBtn = uiDoc.rootVisualElement.Q<VisualElement>("LoadAsset");
        loadBtn.RegisterCallback<ClickEvent>((ClickEvent evt) =>
        {
            LoadAssetTest(pathLabel.text, objectLabel.text, isSceneToggle.value);
        });

    }

    async void LoadAssetTest(string assetsDir,string assetName,bool isScene)
    {
        loader_ = new AssetBundleLoader();
        loader_.Init(assetsDir);

        await loader_.LoadAssetBundleLaunchConfig();

        if (!loader_.CheckPluginRequirements())
        {
            Debug.LogError("Plugins check fail.");
        }

        if (!isScene)
        {

            var assetPfb = await loader_.LoadAssetAsync<GameObject>(assetName);
            GameObject.Instantiate<GameObject>(assetPfb);
        }
        else
        {
            // load scene
            var sceneBundle = await loader_.LoadAssetBundleAsync(loader_.GetAssetBundleWithAssetName(assetName));
            SceneManager.LoadScene(assetName);
        }


    }

    async void LoadSceneTest(string assetsDir, string sceneName)
    {
        loader_ = new AssetBundleLoader();
        loader_.Init(assetsDir);

        await loader_.LoadAssetBundleLaunchConfig();

        if (!loader_.CheckPluginRequirements())
        {
            Debug.LogError("Plugins check fail.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
