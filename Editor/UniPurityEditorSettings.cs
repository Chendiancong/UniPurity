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
                if (mInstance is null)
                {
                    var objs = InternalEditorUtility.LoadSerializedFileAndForget(GetFilePath());
                    mInstance = objs.Length > 0 ? (UniPurityEditorSettings)objs[0] : CreateInstance<UniPurityEditorSettings>();
                }
                return mInstance;
            }
        }

        [Header("补充元数据的aot dll程序集")]
        [Tooltip("主要是针对热更代码调用aot泛型时没有定义的问题，一般来说可能会用到System的泛型容器，当然也可以按需添加或者减少")]
        public string[] staticNeededAOTAssemblies = new string[] { "mscorlib", "System", "System.Core" };

        [Header("额外补充元数据的aot dll程序集")]
        [Tooltip("主要是针对热更代码调用aot泛型时没有定义的问题，一般来说可能会用到System的泛型容器，当然也可以按需添加或者减少")]
        public string[] neededAOTAssemblies;

        public void Save()
        {
            if (mInstance is null)
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
            return new List<string>(staticNeededAOTAssemblies)
                .Concat(neededAOTAssemblies)
                .Select(name => name + ".dll");
        }

        protected static string GetFilePath() => "ProjectSettings/UniPuritySettings.asset";
    }
}