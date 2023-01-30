using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEditorInternal;

namespace UniPurity.Editor
{
    public class UniPurityEditorSettings : ScriptableObject
    {
        private static UniPurityEditorSettings mInstance;
        public static UniPurityEditorSettings Instance
        {
            get
            {
                if (!mInstance)
                {
                    mInstance = NewIns();
                }
                return mInstance;
            }
        }

        public static UniPurityEditorSettings NewIns()
        {
            var objs = InternalEditorUtility.LoadSerializedFileAndForget(GetFilePath());
            var instance = objs.Length > 0 ?
                objs[0] as UniPurityEditorSettings :
                CreateInstance<UniPurityEditorSettings>();
            return instance;
        }

        [Header("����Ԫ���ݵ�aot dll����")]
        [Tooltip("��Ҫ������ȸ��������aot����ʱû�ж�������⣬һ����˵���ܻ��õ�System�ķ�����������ȻҲ���԰�����ӻ��߼���")]
        public string[] staticNeededAOTAssemblies = new string[] { "mscorlib", "System", "System.Core" };

        [Header("���ⲹ��Ԫ���ݵ�aot dll����")]
        [Tooltip("��Ҫ������ȸ��������aot����ʱû�ж�������⣬һ����˵���ܻ��õ�System�ķ�����������ȻҲ���԰�����ӻ��߼���")]
        public string[] neededAOTAssemblies;

        [Header("Ĭ�ϵ�dllĿ¼")]
        [Tooltip("dll���ջᱻ��������Ŀ¼")]
        public string defaultDllPath = $"{Application.streamingAssetsPath}/GameDlls/";

        [Header("�Զ���dllĿ¼")]
        [Tooltip("dll���ջᱻ��������Ŀ¼���˴����������Զ����Ŀ¼������Ĭ��Ŀ¼")]
        public string customDllPath = "";

        public void Save()
        {
            if (!mInstance)
                return;
            string filePath = GetFilePath();
            string directoryName = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directoryName))
                Directory.CreateDirectory(directoryName);
            Object[] objs = new Object[] { mInstance };
            InternalEditorUtility.SaveToSerializedFileAndForget(objs, filePath, true);
        }

        public IEnumerable<string> GetAllNeedAOTFiles()
        {
            if (neededAOTAssemblies is null)
                neededAOTAssemblies = new string[0];
            return new List<string>(staticNeededAOTAssemblies)
                .Concat(neededAOTAssemblies)
                .Select(name => name + ".dll");
        }

        public string GetDllPath()
        {
            return string.IsNullOrEmpty(customDllPath) ?
                defaultDllPath :
                customDllPath;
        }

        protected static string GetFilePath() => "ProjectSettings/UniPuritySettings.asset";
    }
}