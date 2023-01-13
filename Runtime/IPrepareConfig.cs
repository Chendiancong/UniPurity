namespace UniPurity
{
    public interface IPrepareConfig
    {
        /// <summary>
        /// AOTDll清单文件的url
        /// </summary>
        string AOTDllManifestUrl { get; }
        /// <summary>
        /// AOTDll的url，通过url+dllName的方式得到最终的dll下载地址
        /// </summary>
        string AOTDllUrl { get; }
        /// <summary>
        /// 热更Dll清单文件的url
        /// </summary>
        string HotUpdateDllManifestUrl { get; }
        /// <summary>
        /// 热更Dll的url，通过url+dllName的方式得到最终的下载地址
        /// </summary>
        string HotUpdateDllUrl { get; }
    }
}