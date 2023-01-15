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
        [MenuItem("UniPurity/Move HybridCLR Dlls/ActiveTarget", priority = 100)]
        public static void MoveCurrentTargetDll() => MoveDllToStreamingAssets(EditorUserBuildSettings.activeBuildTarget);

        [MenuItem("UniPurity/Move HybridCLR Dlls/Win32", priority = 200)]
        public static void MoveWin32Dll() => MoveDllToStreamingAssets(BuildTarget.StandaloneWindows);

        [MenuItem("UniPurity/Move HybridCLR Dlls/Win64", priority = 201)]
        public static void MoveWin64Dll() => MoveDllToStreamingAssets(BuildTarget.StandaloneWindows64);

        [MenuItem("UniPurity/Move HybridCLR Dlls/Android", priority = 202)]
        public static void MoveAndroidDll() => MoveDllToStreamingAssets(BuildTarget.Android);

        [MenuItem("UniPurity/Move HybridCLR Dlls/IOS", priority = 203)]
        public static void MoveIOSDll() => MoveDllToStreamingAssets(BuildTarget.iOS);

        [MenuItem("UniPurity/Build/BuildAOT", priority = 300)]
        public static void BuildAOT()
        {
            PrebuildCommand.GenerateAll();
            MoveCurrentTargetDll();
        }

        [MenuItem("UniPurity/Build/BuildHotUpdate", priority = 301)]
        public static void BuildHotUpdate()
        {
            CompileDllCommand.CompileDllActiveBuildTarget();
            MoveCurrentTargetDll();
        }

        [MenuItem("UniPurity/Build/All", priority = 302)]
        public static void BuildAll()
        {
            PrebuildCommand.GenerateAll();
            CompileDllCommand.CompileDllActiveBuildTarget();
            MoveCurrentTargetDll();
        }

        [MenuItem("UniPurity/Settings", priority = 400)]
        public static void SettingsCommand()
        {
            SettingsService.OpenProjectSettings("Project/UniPurity Settings");
        }


        private static string GetAOTDllPath() =>
            $"{Application.streamingAssetsPath}/GameDlls/AOT";

        private static string GetHotUpdateDllPath() =>
            $"{Application.streamingAssetsPath}/GameDlls/HotUpdate";

        private static string GetAOTDllManifestPath() =>
            $"{Application.streamingAssetsPath}/GameDlls/AOTManifest.data";

        private static string GetHotUpdateManifestPath() =>
            $"{Application.streamingAssetsPath}/GameDlls/HotUpdateManifest.data";

        private static void MoveDllToStreamingAssets(BuildTarget target)
        {
            Debug.Log($"[UniPurityCommands] Moving {target}'s aot and hotupdate dlls to {Application.dataPath}/GameDlls/...");
            var sourcePath = SettingsUtil.GetAssembliesPostIl2CppStripDir(target);
            var targetPath = GetAOTDllPath();
            var manifestPath = GetAOTDllManifestPath();
            CopyDlls(
                sourcePath, targetPath, manifestPath,
                UniPurityEditorSettings.Instance.GetAllNeedAOTFiles()
            );

            sourcePath = SettingsUtil.GetHotUpdateDllsOutputDirByTarget(target);
            targetPath = GetHotUpdateDllPath();
            manifestPath = GetHotUpdateManifestPath();
            CopyDlls(
                sourcePath, targetPath, manifestPath,
                SettingsUtil.HotUpdateAssemblyFilesExcludePreserved
            );

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