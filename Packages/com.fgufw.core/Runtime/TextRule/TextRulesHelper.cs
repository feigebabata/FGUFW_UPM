using System;

namespace FGUFW
{
    /*
    想做代码文本解析功能 最终返回bool值
    数据分:变量(数据源),常量,运算符,优先级(括号)
    数据源用于用反射还是整形Id(#123)
    常量解析后存储为固定值免得每次调用都Parse字符串
    但目前这些符号只能做数值比较
    &(且)的优先级高于|(或)
    所有匹配符号:()=≠>≥<≤∈∉&|#,
    */
    /// <summary>
    /// 规则匹配 用于配表中的触发判定
    /// </summary>
    public static class TextRulesHelper
    {
        public const char Code_DomainStart = '(';
        public const char Code_DomainEnd = ')';
        //---------------------------------------------
        public const char Code_Equal = '=';
        public const char Code_NotEqual = '≠';
        public const char Code_Greater = '>';
        public const char Code_GreaterAndEqual = '≥';
        public const char Code_Less = '<';
        public const char Code_LessAndEqual = '≤';
        //---------------------------------------------
        public const char Code_In = '∈';
        public const char Code_Out = '∉';
        public const char Code_Split = ',';
        //---------------------------------------------
        public const char Code_And = '&';
        public const char Code_Or = '|';
        //---------------------------------------------
        public const char Code_Variate = '#';
        public const char Code_Value = '\0';

        //---------------------------------------------
        public const char Code_Remainder = '%';
        public const char Code_Multiply = '*';
        public const char Code_Divide = '/';
        //---------------------------------------------
        public const char Code_Add = '+';
        public const char Code_Subtract = '-';




        public static int FindCodeOR(ReadOnlySpan<char> text, int start)
        {
            int domainCount = 0;
            for (int i = start; i < text.Length; i++)
            {
                var c = text[i];
                if (c == Code_DomainStart) domainCount++;
                else if (c == Code_DomainEnd) domainCount--;
                else if (c == Code_Or && domainCount == 0) return i;
            }
            return -1;
        }

        public static int FindCodeAND(ReadOnlySpan<char> text, int start)
        {
            int domainCount = 0;
            for (int i = start; i < text.Length; i++)
            {
                var c = text[i];
                if (c == Code_DomainStart) domainCount++;
                else if (c == Code_DomainEnd) domainCount--;
                else if (c == Code_And && domainCount == 0) return i;
            }
            return -1;
        }


        public static bool IsComputeCode(char code)
        {
            return
            code == Code_Add ||
            code == Code_Subtract ||
            code == Code_Multiply ||
            code == Code_Remainder ||
            code == Code_Divide;
        }

        public static int FindComputeL2Code(ReadOnlySpan<char> text, int start)
        {
            int domainCount = 0;
            for (int i = start; i < text.Length; i++)
            {
                var c = text[i];
                if (c == Code_DomainStart) domainCount++;
                else if (c == Code_DomainEnd) domainCount--;

                if (domainCount != 0) continue;

                if (c == Code_Add) return i;
                if (c == Code_Subtract && i != 0 && !IsComputeCode(text[i - 1])) return i;

            }
            return -1;
        }

        public static int FindComputeL1Code(ReadOnlySpan<char> text, int start)
        {
            int domainCount = 0;
            for (int i = start; i < text.Length; i++)
            {
                var c = text[i];
                if (c == Code_DomainStart) domainCount++;
                else if (c == Code_DomainEnd) domainCount--;

                if (domainCount != 0) continue;

                if (c == Code_Multiply) return i;
                if (c == Code_Divide) return i;
                if (c == Code_Remainder) return i;
            }
            return -1;
        }

        public static int FindEqualCode(ReadOnlySpan<char> text, int start)
        {
            int domainCount = 0;
            for (int i = start; i < text.Length; i++)
            {
                var c = text[i];
                if (c == Code_DomainStart) domainCount++;
                else if (c == Code_DomainEnd) domainCount--;

                if (domainCount != 0) continue;

                if (c == Code_Greater) return i;
                if (c == Code_Less) return i;
                if (c == Code_Equal) return i;
                if (c == Code_GreaterAndEqual) return i;
                if (c == Code_LessAndEqual) return i;
                if (c == Code_NotEqual) return i;
            }
            return -1;
        }


        public static int FindInOutCode(ReadOnlySpan<char> text)
        {
            int domainCount = 0;
            for (int i = 0; i < text.Length; i++)
            {
                var c = text[i];
                if (c == Code_DomainStart) domainCount++;
                else if (c == Code_DomainEnd) domainCount--;

                if (domainCount != 0) continue;

                if (c == Code_In) return i;
                if (c == Code_Out) return i;
            }
            return -1;
        }

        public static bool IsValueText(ReadOnlySpan<char> text)
        {
            if (text[0] == TextRulesHelper.Code_Variate)
            {
                for (int i = 1; i < text.Length; i++)
                {
                    var code = text[i];
                    if (IsNotValueCode(code))
                    {
                        return false;
                    }
                }
            }
            else
            {
                for (int i = 0; i < text.Length; i++)
                {
                    var code = text[i];
                    if (IsNotValueCode(code))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public static bool IsNotValueCode(char code)
        {
            return
            code == Code_DomainStart ||
            code == Code_DomainEnd ||

            code == Code_Greater ||
            code == Code_GreaterAndEqual ||
            code == Code_Less ||
            code == Code_LessAndEqual ||
            code == Code_Equal ||
            code == Code_NotEqual ||

            code == Code_In ||
            code == Code_Out ||
            code == Code_Split ||

            code == Code_And ||
            code == Code_Or ||

            code == Code_Variate ||

            code == Code_Remainder ||
            code == Code_Add ||
            code == Code_Subtract ||
            code == Code_Multiply ||
            code == Code_Divide;
        }


    }

}
/*
文本规则说明文档:

符号:
    ( :域起始
    ) :域结束
    = :判断 等于
    ≠ :判断 不等于
    > :判断 大于
    ≥ :判断 大于等于
    < :判断 小于
    ≤ :判断 小于等于
    ∈ :判断 在集合内
    ∉ :判断 不在集合内
    , :集合分隔符
    & :判断复合 并且
    | :判断复合 或者
    # :变量前缀
    % :计算 取余
    * :计算 乘
    / :计算 除
    + :计算 加
    - :计算 减
示例:
    域: (1+2)*3=9 , 1+2*3=7
    集合: 1∈1,2,3,4 , #手机号∈110,120,119 
    变量: #金币>1 , #年龄<#未成年年龄
    取余: 3%2=1 , 4%2=0
*/