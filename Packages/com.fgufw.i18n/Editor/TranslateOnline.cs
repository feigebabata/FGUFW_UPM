using System;
using System.Threading.Tasks;
using FGUFW.ExcelUtils;
using LitJson;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace FGUFW.I18N
{
    public static class TranslateOnline
    {
        public const string I18N_FILE_PATH = "Assets/FGUFW/I18N/I18N.xlsx";
        static bool translating = false;

        [UnityEditor.MenuItem("I18N/在线翻译")]
        private static async void translateOnline()
        {
            if(translating)
            {
                Debug.LogError("任务进行中,请等待或查看Background Tasks");
                return;
            }

            var filePath = Application.dataPath.Replace("Assets",I18N_FILE_PATH);
            Excel execl = default;
            try
            {
                execl = new Excel(filePath);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"I18N文件打开失败:{I18N_FILE_PATH} \n{ex.Message}");
                return;
            }

            
            translating = true;
            int progressId = Progress.Start("在线翻译多语言配置表");

            var sheet = execl[0];
            var language_codes = sheet.GetRow(2);

            var sl = language_codes.GetCell(1).ToString();

            int rowCount = sheet.LastRowNum + 1;
            int cellCount = language_codes.LastCellNum;
            int uidCount = rowCount-3;

            var totalCount = (cellCount-2)*uidCount;
            var currentIndex = 0;

            for (int language_idx = 2; language_idx < cellCount; language_idx++)
            {
                var tl = language_codes.GetCell(language_idx).ToString();
                for (int uidIdx = 3; uidIdx < rowCount; uidIdx++)
                {
                    var q = sheet.GetRow(uidIdx).GetCell(1).ToString();
                    if(q.IsNull())continue;

                    var newText = await Translate(sl,tl,q);
                    var row = sheet.GetRow(uidIdx);

                    var cell = row.GetCell(language_idx);
                    if(cell==default)
                    {
                        cell = row.CreateCell(language_idx);
                    }
                    cell.SetCellValue(newText);

                    currentIndex++;
                    Progress.Report(progressId, currentIndex / (float)totalCount,$"在线翻译多语言:{currentIndex}/{totalCount}");
                }
            }

            execl.Save();
            
            Progress.Remove(progressId);
            translating = false;
            Debug.Log("I18N 在线翻译结束!");
        }

        private static async Task<string> Translate(string sl,string tl,string q)
        {
            q = Uri.EscapeUriString(q);
            var url = $"https://translate.googleapis.com/translate_a/single?client=gtx&dt=t&sl={sl}&tl={tl}&q={q}";
            UnityWebRequest uwr = new UnityWebRequest(url);

            uwr.downloadHandler = new DownloadHandlerBuffer();
            await uwr.RequestAsync();
            
            var newText = string.Empty;
            try
            {
                var jsonData = JsonMapper.ToObject(uwr.downloadHandler.text);
                foreach (JsonData item in jsonData[0])
                {
                    newText = newText+item[0].ToString();
                }

            }
            catch
            {
                Debug.LogError($"{q}\n{uwr.downloadHandler.text}\n{uwr.error}");
            }

            return newText;
        }
    }
}