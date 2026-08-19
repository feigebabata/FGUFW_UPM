# FGUFW.ExcelUtils
- Excel导表工具
- 最新upm url:https://github.com/feigebabata/FGUFW_UPM.git?path=/Packages/com.fgufw.excel-utils#excel-utils@1.0.1

## 介绍
- 不处理正在打开的Excel文件
- 只识别EC结尾的Excel文件
- 只处理第一行第一列为GenerateExcelCsharpCode的表单 第一行第二列为类型 List/Table/Single
- 在自动导入或手动Reimport文件触发自动转换
- 自动转换会生成对应的Csharp文件和集中放置的Json文件 [Assets/ECJsonData]
- 支持字段类型:string,int,float,bool

## List/Table
- 第二行备注 第三行字段类型 第四行字段名 第五行开始配置数据

## Single
- 第二行为列备注 第三行开始配置数据 第一列为类型 第二列字段名 第三列数据 第四列备注不转换