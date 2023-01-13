using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using HybridCLR;
using HotUpdate;

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
#if UNITY_EDITOR
        StartGame();
#else
        StartCoroutine(DownLoadAssets(StartGame));
#endif
    }

    private void StartGame()
    {
#if UNITY_EDITOR
#else
        LoadMetadataForAOTAssemblies();
        System.Reflection.Assembly.Load(GetAssetData("HotUpdate.dll"));
#endif
        var hotupdate = new HotUpdateMain();
        hotupdate.Show();
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

    /// <summary>
    /// Ϊaot assembly����ԭʼmetadata����������aot�����ȸ��¶���
    /// һ�����غ����AOT���ͺ�����Ӧnativeʵ�ֲ����ڣ����Զ��滻Ϊ����ģʽִ��
    /// </summary>
    private void LoadMetadataForAOTAssemblies()
    {
        /// ע�⣬����Ԫ�����Ǹ�AOT dll����Ԫ���ݣ������Ǹ��ȸ���dll����Ԫ����
        /// �ȸ���dll��ȱԪ���ݣ�����Ҫ���䣬�������LoadMetadataForAOTAssembly�᷵�ش���
        HomologousImageMode mode = HomologousImageMode.SuperSet;
        foreach (var aotDllNames in mAOTMetaAssemblyNames)
        {
            byte[] dllBytes = GetAssetData(aotDllNames);
            // ����assembly��Ӧ��dll�����Զ�Ϊ��hook��һ��aot���ͺ�����native���������ڣ��ý������汾�Ĵ���
            LoadImageErrorCode err = RuntimeApi.LoadMetadataForAOTAssembly(dllBytes, mode);
            Debug.Log($"LoadMetadataForAOTAssembly:{aotDllNames}. mode:{mode} ret:{err}");
        }
    }
}
