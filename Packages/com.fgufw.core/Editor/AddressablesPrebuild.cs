// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEditor;
// using System.IO;
// using FGUFW;
// using System;
// using UnityEditor.Build;
// using UnityEditor.Build.Reporting;
// using UnityEngine.AddressableAssets;
// using UnityEditor.AddressableAssets;
// using UnityEditor.AddressableAssets.Settings;
// using UnityEngine.ResourceManagement.AsyncOperations;
// using UnityEditor.AddressableAssets.Build;
// using System.Linq;

// namespace FGUFW.Editor
// {
//     public class AddressablesPrebuild : IPreprocessBuildWithReport//IPostprocessBuildWithReport
//     {
//         public int callbackOrder => 0; // 执行顺序，数值越小越先执行

//         public void OnPreprocessBuild(BuildReport report)
//         {
//             // BuildAB();  无需再次打包 AddressablesPlayerBuildProcessor文件中已处理
//         }

//         [MenuItem("EditorUtils/Build/AB")]
//         public static void BuildAB()
//         {
//             Debug.Log($"{DateTime.Now.SecondTickName()}开始打包Addressables资源...");

//             AddressablesPlayerBuildResult result;
//             AddressableAssetSettings.BuildPlayerContent(out result);
//             if(!result.Error.IsNull())
//             {
//                 Debug.LogError(result.Error);
//                 return;
//             }

//             Debug.Log($"{DateTime.Now.SecondTickName()}Addressables打包完成！");
//         }

//         [MenuItem("EditorUtils/Build/App")]
//         public static void BuildApp()
//         {
//             Debug.Log($"{DateTime.Now.SecondTickName()}开始打包App...");
//             // BuildPlayerOptions options = BuildPlayerWindow.DefaultBuildMethods.GetBuildPlayerOptions(new BuildPlayerOptions());
//             // options.locationPathName = GetBuildTargetOutPath(options.target);
//             BuildPipeline.BuildPlayer(GetCurrentBuildSettings());
//             Debug.Log($"{DateTime.Now.SecondTickName()}App打包完成！");
//         }

//         public static string GetBuildTargetOutPath(BuildTarget target)
//         {
//             var path = Path.Combine(Path.GetDirectoryName(Application.dataPath),$"Build/{target}/{DateTime.Now.SecondTickName()}");

//             switch (target)
//             {
//                 case BuildTarget.Android:
//                     path = Path.Combine(path,".apk");
//                 break;
//                 case BuildTarget.StandaloneWindows64:
//                 case BuildTarget.StandaloneWindows:
//                     path = Path.Combine(path,$"{Application.productName}.exe");
//                 break;
//             }

//             string directory;

//             if(Path.GetExtension(path).IsNull())
//             {
//                 directory = path;
//             }
//             else
//             {
//                 directory = Path.GetDirectoryName(path);
//             }
//             if(!Directory.Exists(directory))Directory.CreateDirectory(directory);

//             return path;
//         }

//         static BuildPlayerOptions GetCurrentBuildSettings()
//         {
//             return new BuildPlayerOptions
//             {
//                 scenes = EditorBuildSettings.scenes
//                     .Where(scene => scene.enabled)
//                     .Select(scene => scene.path)
//                     .ToArray(),  // 获取所有启用的场景路径
//                 locationPathName = GetBuildTargetOutPath(EditorUserBuildSettings.activeBuildTarget),
//                 target = EditorUserBuildSettings.activeBuildTarget,  // 当前激活平台
//                 options = BuildOptions.None
//             };
//         }

//     }
// }
