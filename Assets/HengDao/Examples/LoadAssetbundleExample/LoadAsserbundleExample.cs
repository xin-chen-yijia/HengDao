using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HengDao;

public class LoadAsserbundleExample : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        AssetBundleLoader loader = new AssetBundleLoader();

        //loader.Init("C:/Temp/test2"); // assetbundles folder
        //loader.Init("file:///C:/Temp/test2"); // assetbundles folder
        loader.Init("http://127.0.0.1:3333/test"); // assetbundles folder

        loader.LoadAssetAsync<GameObject>("Cube",(loadedObj)=>
        {
            Instantiate<GameObject>(loadedObj);
        },null);

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
