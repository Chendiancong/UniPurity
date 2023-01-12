using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using HybridCLR.Editor;

namespace PureUni.Editor
{
    internal static class PureUniCommands
    {
        [MenuItem("PureUni/Move HybridCLR Dlls/ActiveTarget", priority = 100)]
        public static void MoveCurrentTargetDll() => MoveDllToStreamingAssets(EditorUserBuildSettings.activeBuildTarget);

        [MenuItem("PureUni/Move HybridCLR Dlls/Win32", priority = 200)]
        public static void MoveWin32Dll() => MoveDllToStreamingAssets(BuildTarget.StandaloneWindows);

        [MenuItem("PureUni/Move HybridCLR Dlls/Win64", priority = 201)]
        public static void MoveWin64Dll() => MoveDllToStreamingAssets(BuildTarget.StandaloneWindows64);

        [MenuItem("PureUni/Move HybridCLR Dlls/Android", priority = 202)]
        public static void MoveAndroidDll() => MoveDllToStreamingAssets(BuildTarget.Android);

        [MenuItem("PureUni/Move HybridCLR Dlls/IOS", priority = 203)]
        public static void MoveIOSDll() => MoveDllToStreamingAssets(BuildTarget.iOS);


        private static string GetAOTDllPath(BuildTarget target) =>
            $"{Application.streamingAssetsPath}/GameDlls/{target}/AOT";

        private static string GetHotUpdateDllPath(BuildTarget target) =>
            $"{Application.streamingAssetsPath}/GameDlls/{target}/HotUpdate";

        private static string GetAOTDllManifestPath(BuildTarget target) =>
            $"{Application.streamingAssetsPath}/{target}_AOTManifest.data";

        private static string GetHotUpdateManifestPath(BuildTarget target) =>
            $"{Application.streamingAssetsPath}/{target}_HotUpdateManifest.data";

        private static void MoveDllToStreamingAssets(BuildTarget target)
        {
            Debug.Log($"Moving {target}'s aot and hotupdate dlls to {Application.dataPath}/GameDlls/...");
            var sourcePath = SettingsUtil.GetAssembliesPostIl2CppStripDir(target);
            var targetPath = GetAOTDllPath(target);
            var manifestPath = GetAOTDllManifestPath(target);
            CopyDlls(sourcePath, targetPath, manifestPath);

            sourcePath = SettingsUtil.GetHotUpdateDllsOutputDirByTarget(target);
            targetPath = GetHotUpdateDllPath(target);
            manifestPath = GetHotUpdateManifestPath(target);
            CopyDlls(sourcePath, targetPath, manifestPath, new string[] { "Assembly-CSharp.dll" });

            AssetDatabase.Refresh();
        }

        [MenuItem("PureUni/Test")]
        public static void Test()
        {
            Debug.Log($"Create directory {Application.streamingAssetsPath}/aotdlls");
            Directory.CreateDirectory($"{Application.streamingAssetsPath}/aotdlls");
            AssetDatabase.Refresh();
        }

        private static void CopyDlls(string sourcePath, string targetPath, string manifestPath, IEnumerable<string> excludes = null)
        {
            if (!Directory.Exists(sourcePath))
                throw new IOException("Source direction not exist");
            if (Directory.Exists(targetPath))
                Directory.Delete(targetPath, true);
            Directory.CreateDirectory(targetPath);
            var files = Directory.GetFiles(sourcePath, "*.dll");
            var dontCopies = new HashSet<string>();
            var md5Dic = new Dictionary<string, string>();
            if (!(excludes is null))
            {
                foreach (var ex in excludes)
                    dontCopies.Add(ex);
            }
            foreach (var f in files)
            {
                string fName = Path.GetFileName(f);
                if (dontCopies.Contains(fName))
                    continue;
                File.Copy($"{sourcePath}/{fName}", $"{targetPath}/{fName}.byte");
                md5Dic[$"{fName}.byte"] = Utils.Md5File(f);
            }

            using (var fs = new FileStream(manifestPath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                foreach (var kv in md5Dic)
                    fs.Write(Encoding.UTF8.GetBytes($"{kv.Key}:{kv.Value}\n"));
                fs.Close();
            }
        }

        private static void CreateDllAssetBundles(string sourcePath, string targetPath, BuildTarget buildTarget)
        {
            if (!Directory.Exists(sourcePath))
                throw new IOException("Source directory not exist");
            if (Directory.Exists(targetPath))
            {
                var originFiles = Directory.GetFiles(targetPath);
                foreach (var fpath in originFiles)
                    File.Delete(fpath);
            }
            else
                Directory.CreateDirectory(targetPath);

            var files = new List<string>(Directory.GetFiles(sourcePath, "*.dll.byte"));
            var build = new AssetBundleBuild();
            var assetNames = from f in files
                             where !f.EndsWith(".meta")
                             select f.Replace(Application.dataPath, "Assets") into f1
                             select f1.Replace("\\", "/");
            build.assetBundleName = $"aotdlls";
            build.assetNames = assetNames.ToArray();
            foreach (var name in build.assetNames)
                Debug.Log(name);
            BuildPipeline.BuildAssetBundles(
                targetPath,
                new AssetBundleBuild[] { build },
                BuildAssetBundleOptions.ForceRebuildAssetBundle,
                buildTarget
            );
        }
    }
}