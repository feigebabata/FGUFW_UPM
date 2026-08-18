using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using UnityEngine;

namespace FGUFW.ExcelUtils
{
    public class Excel : IEnumerable<ISheet>,IDisposable
    {
        IWorkbook _workbook;

        public int SheetCount => _workbook.NumberOfSheets;

        private string _excelPath;

        private Excel(){}

        public Excel(string excelPath)
        {
            _excelPath = excelPath;
            // 判断 Excel 格式
            // try
            // {
                using (FileStream stream = new FileStream(excelPath, FileMode.Open, FileAccess.Read))
                {
                    if (excelPath.EndsWith(".xls"))
                    {
                        _workbook = new HSSFWorkbook(stream);
                    }
                    else if (excelPath.EndsWith(".xlsx"))
                    {
                        _workbook = new XSSFWorkbook(stream);
                    }
                }
            // }
            // catch (System.Exception ex)
            // {
            //     Debug.LogError(ex.Message);
            // }

        }

        public ISheet this[string name]
        {
            get
            {
                return _workbook.GetSheet(name);
            }
        }

        public ISheet this[int index]
        {
            get
            {
                return _workbook.GetSheetAt(index);
            }
        }

        public IEnumerator<ISheet> GetEnumerator()
        {
            return _workbook.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Dispose()
        {
            _workbook.Close();
        }

        public void Save()
        {
            var tempPath = $"{_excelPath}.temp";
            try
            {
                using (FileStream stream = new FileStream(tempPath, FileMode.Create, FileAccess.Write))
                {
                    _workbook.Write(stream);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError(ex.Message);
                return;
            }

            File.Delete(_excelPath);
            File.Copy(tempPath,_excelPath);
            File.Delete(tempPath);
            
            _workbook.Close();
        }
    }
}