namespace UniPurity
{
    public interface IPrepareConfig
    {
        /// <summary>
        /// AOTDll�嵥�ļ���url
        /// </summary>
        string AOTDllManifestUrl { get; }
        /// <summary>
        /// AOTDll��url��ͨ��url+dllName�ķ�ʽ�õ����յ�dll���ص�ַ
        /// </summary>
        string AOTDllUrl { get; }
        /// <summary>
        /// �ȸ�Dll�嵥�ļ���url
        /// </summary>
        string HotUpdateDllManifestUrl { get; }
        /// <summary>
        /// �ȸ�Dll��url��ͨ��url+dllName�ķ�ʽ�õ����յ����ص�ַ
        /// </summary>
        string HotUpdateDllUrl { get; }
    }
}