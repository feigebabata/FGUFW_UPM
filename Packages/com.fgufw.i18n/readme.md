# I18N
- 国际化多语言模块
- 多语言excel表位置问题待优化

## Packages\manifest.json
```
"com.fgufw.i18n": "https://github.com/feigebabata/FGUFW_UPM.git?path=/Packages/com.fgufw.i18n#i18n@1.0.2",
"com.fgufw.excel-utils": "https://github.com/feigebabata/FGUFW_UPM.git?path=/Packages/com.fgufw.excel-utils#excel-utils@1.0.1",
"com.fgufw.core": "https://github.com/feigebabata/FGUFW_UPM.git?path=/Packages/com.fgufw.core#core@1.0.0",
"com.fgufw.litjson": "https://github.com/feigebabata/FGUFW_UPM.git?path=/Packages/com.fgufw.litjson#litjson@1.0.1"
```

## I18N.xlsx格式:
~~~
语言    ,简体中文,英文    ,日语
标题    ,中文    ,English,にほん
uid\编码,zh-cn   ,en-us  ,ja-jp
~~~

## 各语言编码:
~~~
简体中文,繁体中文,英文   ,日语  ,德语    ,西班牙语,葡萄牙语-巴西,韩语 ,法语     ,俄语
中文    ,繁体中文,English,にほん,Deutsch,Español,Português   ,한국어,Français,Русский
zh-cn   ,zh-hk  ,en-us  ,ja-jp,de-de  ,es-ar  ,pt-br       ,ko-kr ,fr-fr   ,ru-ru
~~~

## I18N.Json
~~~
{
    "标题1":
    {
        "uid1":"翻译结果1",
        "uid2":"翻译结果2"
    },
    "标题2":
    {
        "uid2":"翻译结果2",
        "uid2":"翻译结果2"
    }
}
~~~