namespace UniPurity
{
    public interface IPrepareConfig
    {
        /// <summary>
        /// AOTDll清单文件本地存放目录
        /// </summary>
        string AOTDllManifestLocalPath { get; }
        /// <summary>
        /// AOTDll清单文件url
        /// </summary>
        string AOTDllManifestRemoteUrl { get; }
        /// <summary>
        /// AOTDll文件的本地目录，
        /// 通过path/dllName得方式得到最终的目录
        /// </summary>
        string AOTDllLocalPath { get; }
        /// <summary>
        /// AOTDll的url，
        /// 通过url/dllName的方式得到最终的地址
        /// </summary>
        string AOTDllRemoteUrl { get; }
        /// <summary>
        /// 热更Dll清单文件的本地存放目录
        /// </summary>
        string HotUpdateDllManifestLocalPath { get; }
        /// <summary>
        /// 热更Dll清单文件的url
        /// </summary>
        string HotUpdateDllManifestRemoteUrl { get; }
        /// <summary>
        /// 热更Dll文件的本地目录，
        /// 通过path/dllName的方式得到最终的目录
        /// </summary>
        string HotUpdateDllLocalPath { get; }
        /// <summary>
        /// 热更Dll的url，
        /// 通过url/dllName的方式得到最终的地址
        /// </summary>
        string HotUpdateDllRemoteUrl { get; }
    }
}