using System.Text;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using HybridCLR.Editor;
using HybridCLR.Editor.Commands;

namespace UniPurity.Editor
{
    internal static class UniPurityCommands
    {
        public static void BuildAOTWithTarget(BuildTarget target)
        {
            Debug.Log($"[UniPurityCommands] Build aot dlls for {target}");
            StripAOTDllCommand.GenerateStripedAOTDlls(target, EditorUserBuildSettings.selectedBuildTargetGroup);
            MethodBridgeGeneratorCommand.GenerateMethodBridge(target);
            ReversePInvokeWrapperGeneratorCommand.GenerateReversePInvokeWrapper(target);
            AOTReferenceGeneratorCommand.GenerateAOTGenericReference(target);
        }

        public static void BuildHotUpdateWithTarget(BuildTarget target)
        {
            Debug.Log($"[UniPurityCommands] Build hotupdate dlls for {target}");
            CompileDllCommand.CompileDll(target);
            Il2CppDefGeneratorCommand.GenerateIl2CppDef();
            LinkGeneratorCommand.GenerateLinkXml(target);
        }

        public static void BuildAllWithTarget(BuildTarget target)
        {
            BuildHotUpdateWithTarget(target);
            BuildAOTWithTarget(target);
        }

        public static void CopyAOTDllWithTarget(BuildTarget target)
        {
            MoveDllToTargetPath(target, mActionCopyAOTDll);
        }

        public static void CopyHotUpdateDllWithTarget(BuildTarget target)
        {
            MoveDllToTargetPath(target, mActionCopyHotUpdateDll);
        }

        private static string GetAOTDllPath() =>
            $"{UniPurityEditorSettings.Instance.GetDllPath()}/AOT";

        private static string GetHotUpdateDllPath() =>
            $"{UniPurityEditorSettings.Instance.GetDllPath()}/HotUpdate";

        private static string GetAOTDllManifestPath() =>
            $"{UniPurityEditorSettings.Instance.GetDllPath()}/AOTManifest.data";

        private static string GetHotUpdateManifestPath() =>
            $"{UniPurityEditorSettings.Instance.GetDllPath()}/HotUpdateManifest.data";

        private static readonly int mActionCopyAOTDll = 1 << 0;
        private static readonly int mActionCopyHotUpdateDll = 1 << 1;
        private static void MoveDllToTargetPath(BuildTarget target, int action = 1 | 2)
        {
            if ((action & mActionCopyAOTDll) > 0)
            {
                var sourcePath = SettingsUtil.GetAssembliesPostIl2CppStripDir(target);
                var targetPath = GetAOTDllPath();
                var manifestPath = GetAOTDllManifestPath();
                Debug.Log($"[UniPurityCommands] Moving {target}'s aot dlls to {targetPath}");
                CopyDlls(
                    sourcePath, targetPath, manifestPath,
                    UniPurityEditorSettings.Instance.GetAllNeedAOTFiles()
                );
            }

            if ((action & mActionCopyHotUpdateDll) > 0)
            {
                var sourcePath = SettingsUtil.GetHotUpdateDllsOutputDirByTarget(target);
                var targetPath = GetHotUpdateDllPath();
                var manifestPath = GetHotUpdateManifestPath();
                Debug.Log($"[UniPurityCommands] Moving {target}'s hotupdate dlls to {targetPath}");
                CopyDlls(
                    sourcePath, targetPath, manifestPath,
                    SettingsUtil.HotUpdateAssemblyFilesExcludePreserved
                );
            }

            AssetDatabase.Refresh();
        }

        [MenuItem("UniPurity/Test")]
        public static void Test()
        {
            List<string> names = SettingsUtil.HotUpdateAssemblyNamesExcludePreserved;
            List<string> files = SettingsUtil.HotUpdateAssemblyFilesExcludePreserved;
            foreach (var name in names)
                Debug.Log($"name:{name}");
            foreach (var file in files)
                Debug.Log($"file:{file}");
        }

        private static void CopyDlls(string sourcePath, string targetPath, string manifestPath, IEnumerable<string> includes = null)
        {
            if (!Directory.Exists(sourcePath))
                throw new DirectoryNotFoundException($"[UniPurityCommands] {sourcePath}");
            if (Directory.Exists(targetPath))
                Directory.Delete(targetPath, true);
            Directory.CreateDirectory(targetPath);
            var files = Directory.GetFiles(sourcePath, "*.dll");
            var needCopies = new HashSet<string>();
            var md5Dic = new Dictionary<string, string>();
            if (!(includes is null))
            {
                foreach (var ex in includes)
                    needCopies.Add(ex);
            }
            foreach (var f in files)
            {
                string fName = Path.GetFileName(f);
                if (!needCopies.Contains(fName))
                    continue;
                File.Copy($"{sourcePath}/{fName}", $"{targetPath}/{fName}.byte");
                md5Dic[$"{fName}.byte"] = Utils.Md5File(f);
            }

            if (File.Exists(manifestPath))
                File.Delete(manifestPath);
            using (var fs = new FileStream(manifestPath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                foreach (var kv in md5Dic)
                    fs.Write(Encoding.UTF8.GetBytes($"{kv.Key}:{kv.Value}\n"));
                fs.Close();
            }
        }
    }
}