using System.Collections.Generic;

namespace UniPurity
{
    public interface IPrepareProxy
    {
        /// <summary>
        /// Ϊaot assembly����ԭʼmetadata����������aot�����ȸ��¶��У�
        /// һ�����غ����AOT���ͺ�����Ӧ��nativeʵ�ֲ����ڣ����Զ��滻Ϊ����ģʽִ��
        /// </summary>
        void LoadAOTDll(string fileName, byte[] dllBytes);
        /// <summary>
        /// �����ȸ�Dll
        /// </summary>
        void LoadHotUpdateDll(string fileName, byte[] dllBytes);
    }
}