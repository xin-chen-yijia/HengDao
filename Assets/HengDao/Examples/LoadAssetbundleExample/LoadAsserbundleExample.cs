using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HengDao;

public class LoadAsserbundleExample : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        ServiceLocator.Init("./test/hengdao.json"); //mak
        ServiceLocator.GetABLoader().Load<GameObject>("Prefabs/box");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
