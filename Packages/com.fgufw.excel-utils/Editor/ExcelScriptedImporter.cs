using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace FGUFW.ExcelUtils
{
    [ScriptedImporter(0,new string[]{"xls","xlsx"})]
    public class ExcelScriptedImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            var path = ctx.assetPath;

            if(!Path.GetFileNameWithoutExtension(path).EndsWith("EC"))return;
            
            Excel execl = default;
            try
            {
                execl = new Excel(path);
            }
            catch (System.Exception)
            {
                return;
            }

            GenerateExcelCsharpCode.GenerateCsharpCode(execl,path);
            ExcelCsharpToJson.ToJson(execl,path);

            execl.Dispose();
            EditorApplication.delayCall += delayRefresh;
            Debug.Log($"已生成对应脚本和json文件:{path}".RichText(Color.green));
        }

        static void delayRefresh()
        {
            EditorApplication.delayCall -= delayRefresh;
            AssetDatabase.Refresh();
        }

    }
}