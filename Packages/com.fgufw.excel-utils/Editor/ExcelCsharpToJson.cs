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
using UnityEngine.Pool;

namespace FGUFW.ExcelUtils
{
    public static class ExcelCsharpToJson
    {
        public static void ToJson(Excel excel, string path)
        {
            var name = Path.GetFileNameWithoutExtension(path);
            var jsonBuilder = new StringBuilder();

            jsonBuilder.Append('{');
            for (int i = 0; i < excel.SheetCount; i++)
            {
                var sheet = excel[i];

                try
                {
                    sheetToJson(sheet, jsonBuilder);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"ExcelCsharpToJson:{name}.{sheet.SheetName} \n{ex.Message}\n{ex.StackTrace}");
                }

                if (i < excel.SheetCount - 1)
                {
                    jsonBuilder.Append(',');
                }
            }
            jsonBuilder.Append('}');

            var directory = Path.Combine(Application.dataPath, "ECJsonData");

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(Path.Combine(directory, $"{name}.json"), jsonBuilder.ToString());
        }

        private static void sheetToJson(ISheet sheet, StringBuilder jsonBuilder)
        {
            var firstRow = sheet.GetRow(0);
            if (firstRow == default) return;

            //第一行第一列 校验标识
            if (firstRow.GetCell(0)?.ToString() != "GenerateExcelCsharpCode") return;

            var collection = firstRow.GetCell(1)?.ToString();
            if (collection.IsNull()) return;


            //第三行字段类型
            var types = sheet.GetRow(2);
            //第四行字段名
            var names = sheet.GetRow(3);

            int maxRowIdx = sheet.LastRowNum + 1;
            int maxCellIdx = sheet.GetRow(1).LastCellNum;

            //过滤有效列 type列为空则忽略
            var cellIdxs = ListPool<int>.Get();
            for (int ci = 0; ci < maxCellIdx; ci++)
            {
                var tVar = types.GetCell(ci);
                if (tVar == default) continue;
                cellIdxs.Add(ci);
            }

            if (collection == "List")
            {
                jsonBuilder.Append($"\"{sheet.SheetName}s\":");
                jsonBuilder.Append('[');

                for (int ri = 4; ri < maxRowIdx; ri++)
                {
                    var row = sheet.GetRow(ri);

                    jsonBuilder.Append('{');

                    for (int i = 0; i < cellIdxs.Count; i++)
                    {
                        var ci = cellIdxs[i];
                        jsonBuilder.Append($"\"{names.GetCell(ci)}\":");
                        jsonBuilder.Append(getValueByType(types.GetCell(ci).ToString(), row.GetCell(ci)?.ToString()));
                        if (i < cellIdxs.Count - 1)
                        {
                            jsonBuilder.Append(',');
                        }
                    }
                    jsonBuilder.Append('}');

                    if (ri < maxRowIdx - 1)
                    {
                        jsonBuilder.Append(',');
                    }
                }

                jsonBuilder.Append(']');
            }
            else if (collection == "Table")
            {
                jsonBuilder.Append($"\"{sheet.SheetName}s\":");
                jsonBuilder.Append('{');

                for (int ri = 4; ri < maxRowIdx; ri++)
                {
                    var row = sheet.GetRow(ri);

                    jsonBuilder.Append($"\"{row.GetCell(0)}\":");

                    jsonBuilder.Append('{');
                    for (int i = 0; i < cellIdxs.Count; i++)
                    {
                        var ci = cellIdxs[i];

                        jsonBuilder.Append($"\"{names.GetCell(ci)}\":");
                        jsonBuilder.Append(getValueByType(types.GetCell(ci).ToString(), row.GetCell(ci)?.ToString()));
                        if (i < cellIdxs.Count - 1)
                        {
                            jsonBuilder.Append(',');
                        }
                    }
                    jsonBuilder.Append('}');

                    if (ri < maxRowIdx - 1)
                    {
                        jsonBuilder.Append(',');
                    }
                }

                jsonBuilder.Append('}');
            }
            else if (collection == "Single")
            {
                jsonBuilder.Append($"\"{sheet.SheetName}Single\":");
                jsonBuilder.Append('{');

                for (int ri = 2; ri < maxRowIdx; ri++)
                {
                    var row = sheet.GetRow(ri);

                    jsonBuilder.Append($"\"{row.GetCell(1)}\":");
                    jsonBuilder.Append(getValueByType(row.GetCell(0).ToString(), row.GetCell(2)?.ToString()));

                    if (ri < maxRowIdx - 1)
                    {
                        jsonBuilder.Append(',');
                    }
                }

                jsonBuilder.Append('}');
            }

        }

        static string getValueByType(string type, string value)
        {
            type = type.Trim();
            switch (type)
            {
                case "int":
                    {
                        if (value.IsNull())
                        {
                            return "0";
                        }
                        else
                        {
                            return value;
                        }
                    }
                case "float":
                    {
                        if (value.IsNull())
                        {
                            return "0.0";
                        }
                        else
                        {
                            return value;
                        }
                    }
                case "bool":
                    {
                        if (value.IsNull())
                        {
                            return "false";
                        }
                        else
                        {
                            if (value == "0")
                            {
                                return "false";
                            }
                            else if (value == "1")
                            {
                                return "true";
                            }
                            return value;
                        }
                    }
                case "string":
                    {
                        return $"\"{value}\"";
                    }

                default:
                    Debug.LogError($"未知类型:[{type}]");
                    break;
            }

            return default;
        }
    }
}
