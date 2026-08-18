using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using FGUFW;
using System;
using UnityEditor.Build;

namespace FGUFW.Editor
{
    public static class EditorUtil
    {
        public const string META = ".meta";

        public static string[] GetAllAssetPath(string dirPath)
        {
            var dir = Application.dataPath.Replace("Assets",dirPath);
            if(!Directory.Exists(dir))return new string[0];
            int length = 0;
            var flies = Directory.GetFiles(dir);
            foreach (var path in flies)
            {
                if(Path.GetExtension(path)==META)continue;
                length++;
            }
            var paths = new string[length];
            int index = 0;
            foreach (var path in flies)
            {
                if(Path.GetExtension(path)==META)continue;
                paths[index] = path.Replace(Application.dataPath,"Assets").Replace("\\","/");
                index++;
            }
            return paths;
        }

        public static string GetSeleceFolderPath()
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if(string.IsNullOrEmpty(path))
            {
                return null;
            }
            if(!AssetDatabase.IsValidFolder(path))
            {
                path = Path.GetDirectoryName(path).Replace("\\","/");
            }
            return path;
        }

        
        // [MenuItem("Assets/PrintAssetPath")]
        // static private void printAssetPath()
        // {
        //     string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        //     Debug.Log(path);
        // }


        public static BuildTargetGroup GetCurrentBuildTargetGroup()
        {
            var buildTargetGroup = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
            return buildTargetGroup;
        }

        public static NamedBuildTarget GetCurrentNamedBuildTarget()
        {
            var target = EditorUserBuildSettings.activeBuildTarget;
            var group = BuildPipeline.GetBuildTargetGroup(target);
            return NamedBuildTarget.FromBuildTargetGroup(group);
        }
        
        /// <summary>
        /// 获取当前宏配置
        /// </summary>
        /// <returns></returns>
        public static string[] GetScriptingDefineSymbols()
        {
            string[] defines;

            var namedBuildTarget = GetCurrentNamedBuildTarget();
            PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget,out defines);

            return defines;
        }

        /// <summary>
        /// 设置当前宏配置
        /// </summary>
        /// <param name="defines"></param>
        public static void SetScriptingDefineSymbols(string[] defines)
        {
            var namedBuildTarget = GetCurrentNamedBuildTarget();
            PlayerSettings.SetScriptingDefineSymbols(namedBuildTarget,defines);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // EditorUtility.DisplayDialog("修改当前宏配置","记得按ctrl+s保存修改到配置文件!","关闭");
            // PlayerSettings.asset
        }    
        
        
        [MenuItem("EditorUtils/截屏")]
        public static void Capture()
        {
            var fileName = DateTime.Now.ToString("yyyyMMdd_HHmmss")+".png";
            var directory = Path.Combine(Path.GetDirectoryName(Application.dataPath),"Captures");
            var path = Path.Combine(directory,fileName);
            
            if(!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            ScreenCapture.CaptureScreenshot(path);

            Debug.Log($"保存到:{path}");
        }
    }
}