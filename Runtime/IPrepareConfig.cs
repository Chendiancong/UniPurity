namespace UniPurity
{
    public interface IPrepareConfig
    {
        /// <summary>
        /// AOTDll�嵥�ļ����ش��·��
        /// </summary>
        string AOTDllManifestLocalPath { get; }
        /// <summary>
        /// AOTDll�嵥�ļ�url
        /// </summary>
        string AOTDllManifestRemoteUrl { get; }
        /// <summary>
        /// AOTDll�ļ��ı��ش��Ŀ¼��ͨ��path+dllName�ķ�ʽ�õ����յ�·��
        /// </summary>
        string AOTDllLocalPath { get; }
        /// <summary>
        /// AOTDll��url��ͨ��url+dllName�ķ�ʽ�õ����յ�dll���ص�ַ
        /// </summary>
        string AOTDllRemoteUrl { get; }
        /// <summary>
        /// �ȸ�Dll�嵥�ļ����ش��·��
        /// </summary>
        string HotUpdateDllManifestLocalPath { get; }
        /// <summary>
        /// �ȸ�Dll�嵥�ļ���url
        /// </summary>
        string HotUpdateDllManifestRemoteUrl { get; }
        /// <summary>
        /// �ȸ�Dll�ļ��ı��ش��Ŀ¼��ͨ��path+dllName�ķ�ʽ�õ����յ�·��
        /// </summary>
        string HotUpdateDllLocalPath { get; }
        /// <summary>
        /// �ȸ�Dll��url��ͨ��url+dllName�ķ�ʽ�õ����յ����ص�ַ
        /// </summary>
        string HotUpdateDllRemoteUrl { get; }
    }
}