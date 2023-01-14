namespace UniPurity
{
    public interface IPrepareConfig
    {
        /// <summary>
        /// AOTDll�嵥�ļ����ش��Ŀ¼
        /// </summary>
        string AOTDllManifestLocalPath { get; }
        /// <summary>
        /// AOTDll�嵥�ļ�url
        /// </summary>
        string AOTDllManifestRemoteUrl { get; }
        /// <summary>
        /// AOTDll�ļ��ı���Ŀ¼��
        /// ͨ��path/dllName�÷�ʽ�õ����յ�Ŀ¼
        /// </summary>
        string AOTDllLocalPath { get; }
        /// <summary>
        /// AOTDll��url��
        /// ͨ��url/dllName�ķ�ʽ�õ����յĵ�ַ
        /// </summary>
        string AOTDllRemoteUrl { get; }
        /// <summary>
        /// �ȸ�Dll�嵥�ļ��ı��ش��Ŀ¼
        /// </summary>
        string HotUpdateDllManifestLocalPath { get; }
        /// <summary>
        /// �ȸ�Dll�嵥�ļ���url
        /// </summary>
        string HotUpdateDllManifestRemoteUrl { get; }
        /// <summary>
        /// �ȸ�Dll�ļ��ı���Ŀ¼��
        /// ͨ��path/dllName�ķ�ʽ�õ����յ�Ŀ¼
        /// </summary>
        string HotUpdateDllLocalPath { get; }
        /// <summary>
        /// �ȸ�Dll��url��
        /// ͨ��url/dllName�ķ�ʽ�õ����յĵ�ַ
        /// </summary>
        string HotUpdateDllRemoteUrl { get; }
    }
}