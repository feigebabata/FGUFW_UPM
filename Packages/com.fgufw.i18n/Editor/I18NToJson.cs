using System.IO;
using FGUFW.ExcelUtils.Editor;
using LitJson;
using UnityEngine;

namespace FGUFW.I18N
{
    public static class I18NToJson
    {
        
        [UnityEditor.MenuItem("I18N/转Json")]
        private static void toJsonFile()
        {
            var filePath = Application.dataPath.Replace("Assets",TranslateOnline.I18N_FILE_PATH);

            Excel execl = default;
            try
            {
                execl = new Excel(filePath);
            }
            catch (System.Exception ex)
            {
                Debug.LogError(ex.Message);
                return;
            }

            var sheet = execl[0];
            var rowCount = sheet.LastRowNum + 1;
            var names = sheet.GetRow(1);
            var cellCount = names.LastCellNum;

            var jsonData = new JsonData();

            for (int cellIdx = 1; cellIdx < cellCount; cellIdx++)
            {
                var name = names.GetCell(cellIdx).ToString();
                jsonData[name] = new JsonData();
            }

            for (int rowIdx = 3; rowIdx < rowCount; rowIdx++)
            {
                var row = sheet.GetRow(rowIdx);
                var uid = row.GetCell(0).ToString();

                for (int cellIdx = 1; cellIdx < cellCount; cellIdx++)
                {
                    var name = names.GetCell(cellIdx).ToString();
                    var val = row.GetCell(cellIdx).ToString();
                    var languageMap = jsonData[name];
                    languageMap[uid] = val;
                }
            }

            var outPath = Path.Combine(Application.dataPath, "ECJsonData","I18N.json");
            File.WriteAllText(outPath,jsonData.ToJson(true));

            I18N_Utility.Clear();

            Debug.Log($"已生成I18N的json文件".RichText(Color.green));
        }

    }
}