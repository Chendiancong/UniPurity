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

        [Header("补充元数据的aot dll程序集")]
        [Tooltip("主要是针对热更代码调用aot泛型时没有定义的问题，一般来说可能会用到System的泛型容器，当然也可以按需添加或者减少")]
        public string[] staticNeededAOTAssemblies = new string[] { "mscorlib", "System", "System.Core" };

        [Header("额外补充元数据的aot dll程序集")]
        [Tooltip("主要是针对热更代码调用aot泛型时没有定义的问题，一般来说可能会用到System的泛型容器，当然也可以按需添加或者减少")]
        public string[] neededAOTAssemblies;

        [Header("默认的dll目录")]
        [Tooltip("dll最终会被拷贝到此目录")]
        public string defaultDllPath = $"{Application.streamingAssetsPath}/GameDlls/";

        [Header("自定义dll目录")]
        [Tooltip("dll最终会被拷贝到此目录，此处用于设置自定义的目录，覆盖默认目录")]
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