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
        loader.Init("./test/hengdao.json"); //
        loader.Load<GameObject>("Prefabs/box");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
