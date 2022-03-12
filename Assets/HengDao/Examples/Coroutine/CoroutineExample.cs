using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HengDao;

public class CoroutineExample : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        CoroutineLauncher.current.StartCoroutine(Utils.WaitForAndDo(()=>
        {
            return new WaitForEndOfFrame();
        },
        ()=>
        {
            print("hello");
        }));

        CoroutineLauncher.current.StartCoroutine(Utils.WaitForAndDoLoop(() =>
        {
            return new WaitForEndOfFrame();
        },
        () =>
        {
            return Time.realtimeSinceStartup < 5.0f;
        },
        () =>
        {
            print("update ");
        }));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
