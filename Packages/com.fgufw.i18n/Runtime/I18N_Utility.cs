using System.Linq;
using LitJson;
using UnityEngine;

namespace FGUFW.I18N
{
    public static class I18N_Utility
    {
        public const string JSON_FILE_PATH = "Assets/ECJsonData/I18N.json";

        private static JsonData languages;
        private static string languageName;

        private static JsonData getLanguages()
        {
            if(languages==default)
            {
                var jsonText = "";//AddressablesUtility.Load<TextAsset>(JSON_FILE_PATH).text;

                languages = JsonMapper.ToObject(jsonText);

                languageName = languages.Keys.First();
            }
            return languages;
        }

        public static void Clear()
        {
            languages = default;
        }

        public static void SetLanguage(string name)
        {
            getLanguages();
            languageName = name;
        }

        public static string GetLanguage()
        {
            getLanguages();
            return languageName;
        }

        public static string[] GetLanguages()
        {
            return getLanguages().Keys.ToArray();
        }

        public static string GetText(string name,string uid)
        {
            var languageDatas = getLanguages();

            if(name.IsNull())
            {
                return $"404:name empty";
            }
            if(uid.IsNull())
            {
                return $"404:uid empty";
            }

            if(languageDatas.ContainsKey(name))
            {
                var languageMap = languageDatas[name];
                if(languageMap.ContainsKey(uid))
                {
                    return languageMap[uid].ToString();
                }
                return $"404:uid_{uid}";
            }
            return $"404:map_{name}";
        }

        public static string GetText(string uid)
        {
            getLanguages();
            return GetText(languageName,uid);
        }


    }
}
