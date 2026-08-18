using System.Text.RegularExpressions;

namespace FGUFW
{
    public static class RegexUtility
    {
        /// <summary>
        /// 中文字符
        /// </summary>
        public const string CHINESE_RANGE = @"[\u4e00-\u9fa5]";

        /// <summary>
        /// 非空结尾
        /// </summary>
        public const string END_NO_EMPTY = @"\S$";
        
        /// <summary>
        /// 英文名
        /// </summary>
        public const string ENGINE_NAME = @"^[A-Z][a-zA-Z]*([ '-][a-zA-Z]+)*$";

        /// <summary>
        /// 美国电话号
        /// </summary>
        public const string ENGINE_PHONE_NUMBER = @"^(\+1\s?)?($[0-9]{3}$|[0-9]{3})[\s\-]?[0-9]{3}[\s\-]?[0-9]{4}$";

        /// <summary>
        /// 邮箱地址
        /// </summary>
        public const string EMAIL = @"^(?!\.)[a-zA-Z0-9._%+-]+@(?![-.])([a-zA-Z0-9-]+\.)+[a-zA-Z]{2,}$";


    }
}
