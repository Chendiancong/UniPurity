using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using HybridCLR;

public class Preload : MonoBehaviour
{
    private static List<string> mAOTMetaAssemblyNames = new List<string>()
    {
        "mscorlib.dll",
        "System.dll",
        "System.Core.dll"
    };
    private static Dictionary<string, byte[]> mAssetData = new Dictionary<string, byte[]>();

    private static byte[] GetAssetData(string dllName)
    {
        return mAssetData[dllName];
    }

    private void Start()
    {
        StartGame();
    }

    private string GetWebRequestPath(string asset)
    {
        var path = $"{Application.streamingAssetsPath}/{asset}";
        if (!path.Contains("://"))
            path = "file://" + path;
        if (path.EndsWith(".dll"))
            path += ".byte";
        return path;
    }

    private IEnumerator DownLoadAssets(Action onDownloadComplete)
    {
        var assets = new List<string>(mAOTMetaAssemblyNames);

        foreach (var asset in assets)
        {
            string dllPath = GetWebRequestPath(asset);
            Debug.Log($"start download asset:{dllPath}");
            UnityWebRequest wr = UnityWebRequest.Get(dllPath);
            yield return wr.SendWebRequest();
#if UNITY_2020_1_OR_NEWER
            if (wr.result != UnityWebRequest.Result.Success)
                Debug.Log(wr.error);
#else
            if (wr.isHttpError || wr.isNetworkError)
                Debug.Log(wr.error);
#endif
            else
            {
                //Or retrieve results as binary data
                byte[] assetData = wr.downloadHandler.data;
                Debug.Log($"dll:{asset} size:{assetData.Length}");
                mAssetData[asset] = assetData;
            }
        }

        onDownloadComplete();
    }

    private void StartGame()
    {
//        LoadMetadataForAOTAssemblies();
//#if !UNITY_EDITOR
//    System.Reflection.Assembly.Load(GetAssetData("Assembly-CSharp.dll"))
//#endif
        //var hotupdate = new HotUpdateMain();
        //hotupdate.Show();
    }

    /// <summary>
    /// 为aot assembly加载原始metadata，这个代码放aot或者热更新都行
    /// 一旦加载后，如果AOT泛型函数对应native实现不存在，则自动替换为解释模式执行
    /// </summary>
    private void LoadMetadataForAOTAssemblies()
    {
        /// 注意，补充元数据是给AOT dll补充元数据，而不是给热更新dll补充元数据
        /// 热更新dll不缺元数据，不需要补充，如果调用LoadMetadataForAOTAssembly会返回错误
        HomologousImageMode mode = HomologousImageMode.SuperSet;
        foreach (var aotDllNames in mAOTMetaAssemblyNames)
        {
            byte[] dllBytes = GetAssetData(aotDllNames);
            // 加载assembly对应的dll，会自动为它hook。一旦aot泛型函数的native函数不存在，用解释器版本的代码
            LoadImageErrorCode err = RuntimeApi.LoadMetadataForAOTAssembly(dllBytes, mode);
            Debug.Log($"LoadMetadataForAOTAssembly:{aotDllNames}. mode:{mode} ret:{err}");
        }
    }
}
