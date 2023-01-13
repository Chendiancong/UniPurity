using System.Collections.Generic;

namespace UniPurity
{
    public interface IPrepareProxy
    {
        /// <summary>
        /// 为aot assembly加载原始metadata，这个代码放aot或者热更新都行，
        /// 一旦加载后，如果AOT泛型函数对应的native实现不存在，则自动替换为解释模式执行
        /// </summary>
        void LoadAOTDll(string fileName, byte[] dllBytes);
        /// <summary>
        /// 加载热更Dll
        /// </summary>
        void LoadHotUpdateDll(string fileName, byte[] dllBytes);
    }
}