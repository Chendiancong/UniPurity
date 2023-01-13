using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Networking;
using HybridCLR;

namespace UniPurity
{
    public class UniPurityPrepare : IDisposable
    {
        private IPrepareConfig mConfig;
        private IPrepareProxy mProxy;
        private ProgressHandler mOnProgress;
        private LoadedHandler mOnLoaded;
        private ErrorHandler mOnError;
        private MessageHandler mOnMsg;

        public event ProgressHandler OnProgress
        {
            add => mOnProgress += value;
            remove => mOnProgress -= value;
        }

        public event LoadedHandler OnLoaded
        {
            add => mOnLoaded += value;
            remove => mOnLoaded -= value;
        }

        public event ErrorHandler OnError
        {
            add => mOnError += value;
            remove => mOnError -= value;
        }

        public event MessageHandler OnMsg
        {
            add => mOnMsg += value;
            remove => mOnMsg -= value;
        }

        public UniPurityPrepare() : this(new DefaultPrepareConfig(), new DefaultPrepareProxy()) { }

        public UniPurityPrepare(IPrepareConfig config) : this(config, new DefaultPrepareProxy()) { }

        public UniPurityPrepare(IPrepareProxy proxy) : this(new DefaultPrepareConfig(), proxy) { }

        public UniPurityPrepare(IPrepareConfig config, IPrepareProxy proxy)
        {
            mConfig = config;
            mProxy = proxy;
        }

        public void Dispose()
        {
            mOnProgress = null;
            mOnLoaded = null;
            mOnError = null;
            mOnMsg = null;
        }

        public IEnumerator PrepareDlls()
        {
            yield return 0;
            string manifestPath = mConfig.AOTDllManifestLocalPath;
            string manifestRemoteUrl = mConfig.AOTDllManifestRemoteUrl;
            string random = DateTime.Now.ToString("yyyymmddhhmmss");
            manifestRemoteUrl += $"?v={random}";
            PostMessage($"Fetching {manifestRemoteUrl}");

            UnityWebRequest request = UnityWebRequest.Get(manifestRemoteUrl);
            yield return request.SendWebRequest();
            if (!IsRequestSuccess(request))
            {
                var ex = new UniPurityPrepareLoadException()
                {
                    Status = LoadStatus.NetWorkError,
                    FileName = manifestRemoteUrl,
                };
                PostError(ex);
                yield break;
            }
            string[] remoteAOTTags = request.downloadHandler.text.Split('\n');
            foreach (var tag in remoteAOTTags)
                Debug.Log(tag);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void PostMessage(string msg)
        {
            if (!(mOnMsg is null))
                mOnMsg(msg);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void PostError(UniPurityPrepareLoadException ex)
        {
            if (!(mOnError is null))
                mOnError(ex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsRequestSuccess(UnityWebRequest request)
        {
#if UNITY_2020_1_OR_NEWER
            if (request.result != UnityWebRequest.Result.Success)
                return false;
#else
            if (request.isHttpError || request.isNetworkError || !request.isDone)
                return false;
#endif
            return true;
        }

        /// <summary>
        /// dll文件加载进度
        /// </summary>
        public delegate void ProgressHandler(ProgressInfo pi);

        /// <summary>
        /// dll文件加载完成
        /// </summary>
        public delegate void LoadedHandler(IEnumerable<string> allFileNames);

        /// <summary>
        /// dll文件加载出错
        /// </summary>
        public delegate void ErrorHandler(UniPurityPrepareLoadException ex);

        /// <summary>
        /// 消息处理
        /// </summary>
        public delegate void MessageHandler(string message);

        public struct ProgressInfo
        {
            /// <summary>
            /// 文件名
            /// </summary>
            public string fileName;
            /// <summary>
            /// dll分组类型
            /// </summary>
            public LoadDllGroupType groupType;
            /// <summary>
            /// 当前进度
            /// </summary>
            public int cur;
            /// <summary>
            /// 分组总进度
            /// </summary>
            public int groupTotal;
            /// <summary>
            /// 总进度
            /// </summary>
            public int total;
        }

        public class UniPurityPrepareLoadException : Exception
        {
            public LoadStatus Status { get; set; } = LoadStatus.OK;
            public LoadImageErrorCode ErrorCode { get; set; } = LoadImageErrorCode.OK;
            public string FileName { get; set; }

            public UniPurityPrepareLoadException()
            { }

            public UniPurityPrepareLoadException(Exception inner) : base(inner.Message, inner)
            { }
        }

        public enum LoadStatus
        {
            OK = 0,
            NetWorkError,
            AOTAssemblyError,
            HotUpdateAssemblyError
        }

        public enum LoadDllGroupType
        {
            AOT,
            HotUpdate,
        }

        private class DefaultPrepareConfig : IPrepareConfig
        {
            public string AOTDllManifestLocalPath { get; } = $"{Application.streamingAssetsPath}/GameDlls/AOTManifest.data";
            public string AOTDllManifestRemoteUrl { get; } = $"file://{Application.streamingAssetsPath}/GameDlls/AOTManifest.data";
            public string AOTDllLocalPath { get; } = $"{Application.streamingAssetsPath}/GameDlls/AOT/";
            public string AOTDllRemoteUrl { get; } = $"file://{Application.streamingAssetsPath}/GameDlls/AOT/";
            public string HotUpdateDllManifestLocalPath { get; } = $"{Application.streamingAssetsPath}/GameDlls/HotUpdateManifest.data";
            public string HotUpdateDllManifestRemoteUrl { get; } = $"file://{Application.streamingAssetsPath}/GameDlls/HotUpdateManifest.data";
            public string HotUpdateDllLocalPath { get; } = $"{Application.streamingAssetsPath}/GameDlls/HotUpdate/";
            public string HotUpdateDllRemoteUrl { get; } = $"file://{Application.streamingAssetsPath}/GameDlls/HotUpdate/";
        }

        private class DefaultPrepareProxy : IPrepareProxy
        {
            public void LoadAOTDll(string fileName, byte[] dllBytes)
            {
                /// 注意，补充元数据是给AOT dll补充元数据，而不是给热更新dll补充元数据
                /// 热更新dll不缺元数据，不需要补充，如果调用LoadMetadataForAOTAssembly会返回错误
                HomologousImageMode mode = HomologousImageMode.SuperSet;
                LoadImageErrorCode err = RuntimeApi.LoadMetadataForAOTAssembly(dllBytes, mode);
                if (err != LoadImageErrorCode.OK)
                {
                    var ex = new UniPurityPrepareLoadException()
                    {
                        Status = LoadStatus.AOTAssemblyError,
                        ErrorCode = err,
                        FileName = fileName
                    };
                    throw ex;
                }
            }

            public void LoadHotUpdateDll(string fileName, byte[] dllBytes)
            {
                try
                {
                    System.Reflection.Assembly.Load(dllBytes);
                }
                catch (Exception e)
                {
                    throw new UniPurityPrepareLoadException(e)
                    {
                        Status = LoadStatus.HotUpdateAssemblyError,
                        FileName = fileName
                    };
                }
            }
        }
    }
}