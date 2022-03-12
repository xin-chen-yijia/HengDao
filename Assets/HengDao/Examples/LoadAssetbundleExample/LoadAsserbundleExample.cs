using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using HengDao;

public class LoadAsserbundleExample : MonoBehaviour
{
    [SerializeField]
    private TipsWindow tipsWnd;

    // Start is called before the first frame update
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void LoadConfigFile(string filePath)
    {
        if(!System.IO.File.Exists(filePath))
        {
            tipsWnd.SetTips("文件不存在！");
            tipsWnd.Show();
            return;
        }
        ServiceLocator.Init(filePath); //
    }

    public void LoadScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            tipsWnd.SetTips("场景名字不能为空！");
            tipsWnd.Show();
            return;
        }
        SceneLoader.LoadFromAB(sceneName);
    }

    public void LoadAsset(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            tipsWnd.SetTips("资源名字不能为空！");
            tipsWnd.Show();
            return;
        }
        GameObject pfb = ServiceLocator.GetABLoader().LoadAndInstantiate<GameObject>(path);
    }
}
