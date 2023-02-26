using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HengDao;

public class AsyncUIExample : MonoBehaviour
{
    private AsyncAssetBundleLoader loader_;
    // Start is called before the first frame update
    void Start()
    {
        Test();
    }

    async void Test()
    {
        loader_ = new AsyncAssetBundleLoader();
        loader_.Init("c:/Temp/test");

        var res = await loader_.LoadAssetBundleLaunchConfig();
        Debug.Assert(res);

        var uiManager = await AsyncUIManager.Create(loader_);
        uiManager.GetWindow<TestWindow>().Show();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
