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

        //loader.Init("C:/Temp/test"); // assetbundles folder
        //loader.Init("http://127.0.0.1:3333/test"); // assetbundles folder
        loader.Init("file:///C:/Temp/test"); // assetbundles folder

        loader.CheckAssetBundleValid((errId, errMessage) =>
        {
            if(errId == AssetBundleLoader.AssetBundleError.kSucess)
            {
                loader.LoadAssetAsync<GameObject>("Cube", (loadedObj) =>
                {
                    Instantiate<GameObject>(loadedObj);
                }, null);
            }
            else
            {
                Debug.Log("errId:" + errId + " , " + errMessage);
            }
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
