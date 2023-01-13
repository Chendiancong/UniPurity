using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.IO;

namespace UniPurity
{
    public static class Utils
    {
        private readonly static StringBuilder mSb;

        static Utils()
        {
            mSb = new StringBuilder();
        }

        /// <summary>
        /// 计算字符串的md5
        /// </summary>
        public static string Md5(string source)
        {
            MD5 md5Provider = new MD5CryptoServiceProvider();
            byte[] data = Encoding.UTF8.GetBytes(source);
            byte[] md5Data = md5Provider.ComputeHash(data);
            md5Provider.Clear();
            mSb.Clear();
            //for (int i = 0, len = md5Data.Length; i < len; ++i)
            //    mSb.Append(Convert.ToString(md5Data[i], 16).PadLeft(2, '0'));
            //return mSb.ToString().PadLeft(32, '0');
            for (int i = 0, len = md5Data.Length; i < len; ++i)
                mSb.Append(md5Data[i].ToString("x2"));
            return mSb.ToString();
        }

        /// <summary>
        /// 计算文件的md5
        /// </summary>
        public static string Md5File(string filePath)
        {
            try
            {
                FileStream fs = new FileStream(filePath, FileMode.Open);
                MD5 md5Provider = new MD5CryptoServiceProvider();
                byte[] md5Data = md5Provider.ComputeHash(fs);
                fs.Close();
                md5Provider.Clear();
                mSb.Clear();
                for (int i = 0, len = md5Data.Length; i < len; ++i)
                    mSb.Append(md5Data[i].ToString("x2"));
                return mSb.ToString();
            }
            catch { throw; }
        }
    }
}