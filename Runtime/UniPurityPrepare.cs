using System;
using System.Collections.Generic;
using UnityEngine;
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
        }

        public void Load()
        {

        }

        //private IEnumerator CoroutineLoad()
        //{

        //}

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
            private string mAOTDllManifestUrl = $"{Application.streamingAssetsPath}/GameDlls/AOTManifest.data";
            public string AOTDllManifestUrl => mAOTDllManifestUrl;

            private string mAOTDllUrl = $"{Application.streamingAssetsPath}/GameDlls/AOT";
            public string AOTDllUrl => mAOTDllUrl;

            private string mHotUpdateDllManifestUrl = $"{Application.streamingAssetsPath}/GameDlls/HotUpdateManifest.data";
            public string HotUpdateDllManifestUrl => mHotUpdateDllManifestUrl;

            private string mHotUpdateDllUrl = $"{Application.streamingAssetsPath}/GameDlls/HotUpdate";
            public string HotUpdateDllUrl => mHotUpdateDllUrl;
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
#if !UNITY_EDITOR
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
#endif
            }
        }
    }
}