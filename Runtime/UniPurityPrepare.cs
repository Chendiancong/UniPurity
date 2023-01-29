using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.IO;
using System.Linq;
using System.Threading;
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

        public LoadStatus Status { get; private set; } = LoadStatus.NotLoad;

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
            PostMessage("UniPurityPrepare Dispose");
            mOnProgress = null;
            mOnLoaded = null;
            mOnError = null;
            mOnMsg = null;
        }

        public IEnumerator PrepareDlls()
        {
            if (Application.isEditor)
            {
                PostLoaded(new List<string>());
                yield break;
            }

            yield return 0;
            string manifestUrl = $"file:///{mConfig.HotUpdateDllManifestLocalPath}";
            string manifestRemoteUrl = mConfig.HotUpdateDllManifestRemoteUrl;
            string random = DateTime.Now.ToString("yyyymmddhhmmss");

            //读取本地的热更dll清单
            PostMessage($"Fetching {manifestUrl}");
            Dictionary<string, string> localDlls = new Dictionary<string, string>();
            using (var request = UnityWebRequest.Get(manifestUrl))
            {
                yield return request.SendWebRequest();
                CheckRequestFail(request);
                string[] textLines = request.downloadHandler.text.Split('\n');
                SetupDllMd5(textLines, localDlls);
            }

            //下载热更dll清单
            manifestRemoteUrl += $"?v={random}";
            PostMessage($"Fetching {manifestRemoteUrl}");
            Dictionary<string, string> remoteDlls = new Dictionary<string, string>();
            using (var request = UnityWebRequest.Get(manifestRemoteUrl))
            {
                yield return request.SendWebRequest();
                CheckRequestFail(request);
                string[] textLines = request.downloadHandler.text.Split('\n');
                SetupDllMd5(textLines, remoteDlls);
                //写入到本地清单
                File.WriteAllBytes(mConfig.HotUpdateDllManifestLocalPath, request.downloadHandler.data);
            }

            //进行md5对比
            PostMessage("Update hotupdate dlls");
            List<string> needUpdateDllNames = new List<string>(remoteDlls.Count);
            foreach (var kv in remoteDlls)
            {
                string localMd5;
                if (!localDlls.TryGetValue(kv.Key, out localMd5))
                    localMd5 = "";
                bool needUpdate = string.Compare(kv.Value, localMd5, StringComparison.OrdinalIgnoreCase) != 0;
                if (needUpdate)
                    needUpdateDllNames.Add(kv.Key);
            }

            //更新热更dll
            ProgressInfo.sGroupType = LoadDllGroupType.UpdateDll;
            ProgressInfo.sGroupTotal = needUpdateDllNames.Count;
            ProgressInfo.sTotal = needUpdateDllNames.Count;
            using (var group = new WebRequestGroup())
            {
                Dictionary<string, string> url2Name = new Dictionary<string, string>(needUpdateDllNames.Count);
                int counter = 0;
                group.OnComplete += (UnityWebRequest request) =>
                {
                    string dllName;
                    if (!url2Name.TryGetValue(request.url, out dllName))
                        return;
                    if (request.downloadedBytes > 0)
                        File.WriteAllBytes($"{mConfig.HotUpdateDllLocalPath}/{dllName}", request.downloadHandler.data);
                    var pi = ProgressInfo.QuickCreate(dllName, ++counter);
                    PostProgress(ref pi);
                };
                foreach (var dllName in needUpdateDllNames)
                {
                    string remoteUrl = $"{mConfig.HotUpdateDllRemoteUrl}/{dllName}?v={random}";
                    var request = group.Request(remoteUrl);
                    url2Name[request.url] = dllName;
                }
                yield return group;
            }

            string[] aotFiles = Directory.GetFiles(mConfig.AOTDllLocalPath, "*.byte");
            string[] hotupdateFiles = Directory.GetFiles(mConfig.HotUpdateDllLocalPath, "*.byte");
            int aotLen = aotFiles.Length;
            int hotupdateLen = hotupdateFiles.Length;
            //加载aotdll
            ProgressInfo.sGroupType = LoadDllGroupType.AOT;
            ProgressInfo.sGroupTotal = aotLen;
            ProgressInfo.sTotal = aotLen + hotupdateLen;
            using (var group = new WebRequestGroup())
            {
                Dictionary<string, string> url2File = new Dictionary<string, string>();
                int counter = 0;
                group.OnComplete += (UnityWebRequest request) =>
                {
                    string file;
                    if (!url2File.TryGetValue(request.url, out file))
                        return;
                    if (request.downloadedBytes > 0)
                        mProxy.LoadAOTDll(file, request.downloadHandler.data);

                    var pi = ProgressInfo.QuickCreate(file, ++counter);
                    PostProgress(ref pi);
                };

                foreach (var file in aotFiles)
                {
                    string url = $"file:///{file}";
                    var request = group.Request(url);
                    url2File[url] = file;
                }
                yield return group;
            }

            //加载热更dll
            ProgressInfo.sGroupType = LoadDllGroupType.HotUpdate;
            ProgressInfo.sGroupTotal = hotupdateLen;
            using (var group = new WebRequestGroup())
            {
                Dictionary<string, string> url2File = new Dictionary<string, string>();
                int counter = 0;
                group.OnComplete += (UnityWebRequest request) =>
                {
                    string file;
                    if (!url2File.TryGetValue(request.url, out file))
                        return;
                    if (request.downloadedBytes > 0)
                        mProxy.LoadHotUpdateDll(file, request.downloadHandler.data);

                    var pi = ProgressInfo.QuickCreate(file, ++counter);
                    PostProgress(ref pi);
                };

                foreach (var file in hotupdateFiles)
                {
                    string url = $"file:///{file}";
                    var request = group.Request(url);
                    url2File[request.url] = file;
                }
                yield return group;
            }

            PostLoaded(new List<string>(aotFiles).Concat(hotupdateFiles));
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
        private void PostProgress(ref ProgressInfo pi)
        {
            if (!(mOnProgress is null))
                mOnProgress(ref pi);
        }

        private void PostLoaded(IEnumerable<string> allFileNames)
        {
            if (!(mOnLoaded is null))
                mOnLoaded(allFileNames);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CheckRequestFail(UnityWebRequest request)
        {
            bool result = true;
#if UNITY_2020_1_OR_NEWER
            if (request.result != UnityWebRequest.Result.Success)
                result = false;
#else
            if (request.isHttpError || request.isNetworkError || !request.isDone)
                result = false;
#endif
            if (!result)
            {
                var ex = new UniPurityPrepareLoadException($"Fetch {request.url} error")
                {
                    Status = ExceptionStatus.NetWorkError,
                    FileName = request.url
                };
                try { throw ex; }
                catch { throw; }
                finally { PostError(ex); }
            }
        }

        private void SetupDllMd5(string[] dllMd5s, Dictionary<string, string> dic)
        {
            foreach (string dllMd5 in dllMd5s)
            {
                if (string.IsNullOrEmpty(dllMd5))
                    break;
                int idx = dllMd5.IndexOf(':');
                string dllName = dllMd5.Substring(0, idx);
                string md5 = dllMd5.Substring(idx + 1);
                dic[dllName] = md5;
            }
        }

        /// <summary>
        /// dll文件加载进度
        /// </summary>
        public delegate void ProgressHandler(ref ProgressInfo pi);

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
            public static LoadDllGroupType sGroupType = LoadDllGroupType.AOT;
            public static int sGroupTotal = 0;
            public static int sTotal = 0;

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

            /// <summary>
            /// 使用预先设置的静态字段和参数所代表的字段创建ProgressInfo
            /// </summary>
            public static ProgressInfo QuickCreate(string fileName, int cur)
            {
                return new ProgressInfo()
                {
                    fileName = fileName,
                    groupType = sGroupType,
                    cur = cur,
                    groupTotal = sGroupTotal,
                    total = sTotal
                };
            }
        }

        public class UniPurityPrepareLoadException : Exception
        {
            public ExceptionStatus Status { get; set; } = ExceptionStatus.OK;
            public LoadImageErrorCode ErrorCode { get; set; } = LoadImageErrorCode.OK;
            public string FileName { get; set; }

            public UniPurityPrepareLoadException()
            { }

            public UniPurityPrepareLoadException(string message) : base(message)
            { }

            public UniPurityPrepareLoadException(Exception inner) : base(inner.Message, inner)
            { }
        }

        public enum LoadStatus
        {
            NotLoad,
            Loading,
            Loaded,
        }

        public enum ExceptionStatus
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
            UpdateDll,
        }

        private class DefaultPrepareConfig : IPrepareConfig
        {
            public string AOTDllManifestLocalPath { get; } = $"{Application.streamingAssetsPath}/GameDlls/AOTManifest.data";
            public string AOTDllManifestRemoteUrl { get; } = $"file:///{Application.streamingAssetsPath}/GameDlls/AOTManifest.data";
            public string AOTDllLocalPath { get; } = $"{Application.streamingAssetsPath}/GameDlls/AOT/";
            public string AOTDllRemoteUrl { get; } = $"file:///{Application.streamingAssetsPath}/GameDlls/AOT";
            public string HotUpdateDllManifestLocalPath { get; } = $"{Application.streamingAssetsPath}/GameDlls/HotUpdateManifest.data";
            public string HotUpdateDllManifestRemoteUrl { get; } = $"file:///{Application.streamingAssetsPath}/GameDlls/HotUpdateManifest.data";
            public string HotUpdateDllLocalPath { get; } = $"{Application.streamingAssetsPath}/GameDlls/HotUpdate/";
            public string HotUpdateDllRemoteUrl { get; } = $"file:///{Application.streamingAssetsPath}/GameDlls/HotUpdate";
        }

        private class DefaultPrepareProxy : IPrepareProxy
        {
            public void LoadAOTDll(string fileName, byte[] dllBytes)
            {
                /// 注意，补充元数据是给AOT dll补充元数据，而不是给热更新dll补充元数据
                /// 热更新dll不缺元数据，不需要补充，如果调用LoadMetadataForAOTAssembly会返回错误
#if !UNITY_EDITOR
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
#endif
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

        private class InternalInstruction : CustomYieldInstruction
        {
            private Func<bool> checker;

            public override bool keepWaiting => checker is null ? false : checker();

            public InternalInstruction(Func<bool> checker)
            {
                this.checker = checker;
            }
        }
    }
}