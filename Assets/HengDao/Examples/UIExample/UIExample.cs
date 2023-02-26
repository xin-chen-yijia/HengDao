using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HengDao;

public class UIExample : MonoBehaviour
{
    private AssetBundleLoader loader_;
    // Start is called before the first frame update
    void Start()
    {
        Test();
    }

    void Test()
    {
        loader_ = new AssetBundleLoader();
        loader_.Init("c:/Temp/test");

        UIManager uiManager = UIManager.Create(loader_);
        uiManager.GetWindow<TestWindow>().Show();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
