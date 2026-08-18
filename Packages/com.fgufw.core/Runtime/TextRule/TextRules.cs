using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using UnityEngine;
using UnityEngine.Pool;

namespace FGUFW
{
    public class TextRules
    {
        /// <summary>
        /// 符号 计算/比较
        /// </summary>
        private char rule;

        /// <summary>
        /// 是否为变量
        /// </summary>
        private bool isVariate;
        private float value;
        private List<TextRules> children;


        public TextRules(object variateGet, ReadOnlySpan<char> text)
        {
            //去掉空字符
            text = text.Trim();

            //去掉首尾括号
            if (text[0] == TextRulesHelper.Code_DomainStart && text[text.Length - 1] == TextRulesHelper.Code_DomainEnd)
            {
                text = text.Slice(1, text.Length - 2);
            }

            if (TextRulesHelper.IsValueText(text))//纯数值
            {
                if (text[0] == TextRulesHelper.Code_Variate)//变量
                {
                    isVariate = true;
                    value = VariateGetUtility.GetVariateKey(variateGet, text.Slice(1).ToString());
                }
                else//常量
                {
                    isVariate = false;
                    try
                    {
                        value = text.ToFloat();
                    }
                    catch (System.Exception)
                    {
                        Debug.LogError($"无法解析常量:{text.ToString()}");
                    }
                }
            }
            else if (TextRulesHelper.FindCodeOR(text, 0) != -1)// 或
            {
                children = new List<TextRules>(2);
                int idx = TextRulesHelper.FindCodeOR(text, 0);
                rule = text[idx];
                children.Add(new TextRules(variateGet, text.Slice(0, idx)));
                children.Add(new TextRules(variateGet, text.Slice(idx + 1)));
            }
            else if (TextRulesHelper.FindCodeAND(text, 0) != -1)// 且
            {
                children = new List<TextRules>(2);
                int idx = TextRulesHelper.FindCodeAND(text, 0);
                rule = text[idx];
                children.Add(new TextRules(variateGet, text.Slice(0, idx)));
                children.Add(new TextRules(variateGet, text.Slice(idx + 1)));
            }
            else if (TextRulesHelper.FindEqualCode(text, 0) != -1)//比较
            {
                children = new List<TextRules>(2);
                int idx = TextRulesHelper.FindEqualCode(text, 0);
                rule = text[idx];
                children.Add(new TextRules(variateGet, text.Slice(0, idx)));
                children.Add(new TextRules(variateGet, text.Slice(idx + 1)));
            }
            else if (TextRulesHelper.FindComputeL2Code(text, 0) != -1)//加减
            {
                children = new List<TextRules>(2);
                int idx = TextRulesHelper.FindComputeL2Code(text, 0);
                rule = text[idx];
                children.Add(new TextRules(variateGet, text.Slice(0, idx)));
                children.Add(new TextRules(variateGet, text.Slice(idx + 1)));
            }
            else if (TextRulesHelper.FindComputeL1Code(text, 0) != -1)//乘除取余
            {
                children = new List<TextRules>(2);
                int idx = TextRulesHelper.FindComputeL1Code(text, 0);
                rule = text[idx];
                children.Add(new TextRules(variateGet, text.Slice(0, idx)));
                children.Add(new TextRules(variateGet, text.Slice(idx + 1)));
            }
            else if (TextRulesHelper.FindInOutCode(text) != -1) //集合
            {
                children = new List<TextRules>();
                int idx = TextRulesHelper.FindInOutCode(text);
                rule = text[idx];
                children.Add(new TextRules(variateGet, text.Slice(0, idx)));

                var rights = text.Slice(idx + 1).ToString().Split(TextRulesHelper.Code_Split);
                foreach (var item in rights)
                {
                    children.Add(new TextRules(variateGet, item));
                }

            }
            else
            {
                throw new Exception($"unknown textRule:{text.ToString()}");
            }
            
        }


        public float GetValue(object variateGet)
        {
            float result = 0;
            switch (rule)
            {
                case TextRulesHelper.Code_Value:
                    {
                        result = VariateGetUtility.GetValue(variateGet, isVariate, value);
                    }
                    break;
                //----------------------------------------------------
                case TextRulesHelper.Code_Or:
                    {
                        foreach (var child in children)
                        {
                            if (child.GetValue(variateGet) == 1)
                            {
                                result = 1;
                                break;
                            }
                        }
                    }
                    break;
                case TextRulesHelper.Code_And:
                    {
                        result = 1;
                        foreach (var child in children)
                        {
                            if (child.GetValue(variateGet) == 0)
                            {
                                result = 0;
                                break;
                            }
                        }
                    }
                    break;
                //----------------------------------------------------
                case TextRulesHelper.Code_In:
                    {
                        result = 0;
                        var left = children[0].GetValue(variateGet);

                        int length = children.Count;
                        for (int i = 1; i < length; i++)
                        {
                            var right = children[i].GetValue(variateGet);
                            if (left == right)
                            {
                                result = 1;
                                break;
                            }
                        }
                    }
                    break;
                case TextRulesHelper.Code_Out:
                    {
                        result = 1;
                        var left = children[0].GetValue(variateGet);

                        int length = children.Count;
                        for (int i = 1; i < length; i++)
                        {
                            var right = children[i].GetValue(variateGet);
                            if (left == right)
                            {
                                result = 0;
                                break;
                            }
                        }
                    }
                    break;
                //----------------------------------------------------
                case TextRulesHelper.Code_Equal:
                    {
                        var left = children[0].GetValue(variateGet);
                        var right = children[1].GetValue(variateGet);
                        result = left == right ? 1 : 0;
                    }
                    break;
                case TextRulesHelper.Code_NotEqual:
                    {
                        var left = children[0].GetValue(variateGet);
                        var right = children[1].GetValue(variateGet);
                        result = left != right ? 1 : 0;
                    }
                    break;
                case TextRulesHelper.Code_Greater:
                    {
                        var left = children[0].GetValue(variateGet);
                        var right = children[1].GetValue(variateGet);
                        result = left > right ? 1 : 0;
                    }
                    break;
                case TextRulesHelper.Code_Less:
                    {
                        var left = children[0].GetValue(variateGet);
                        var right = children[1].GetValue(variateGet);
                        result = left < right ? 1 : 0;
                    }
                    break;
                case TextRulesHelper.Code_GreaterAndEqual:
                    {
                        var left = children[0].GetValue(variateGet);
                        var right = children[1].GetValue(variateGet);
                        result = left >= right ? 1 : 0;
                    }
                    break;
                case TextRulesHelper.Code_LessAndEqual:
                    {
                        var left = children[0].GetValue(variateGet);
                        var right = children[1].GetValue(variateGet);
                        result = left <= right ? 1 : 0;
                    }
                    break;
                //----------------------------------------------------
                case TextRulesHelper.Code_Add:
                    {
                        var left = children[0].GetValue(variateGet);
                        var right = children[1].GetValue(variateGet);
                        result = left + right;
                    }
                    break;
                case TextRulesHelper.Code_Subtract:
                    {
                        var left = children[0].GetValue(variateGet);
                        var right = children[1].GetValue(variateGet);
                        result = left - right;
                    }
                    break;
                case TextRulesHelper.Code_Multiply:
                    {
                        var left = children[0].GetValue(variateGet);
                        var right = children[1].GetValue(variateGet);
                        result = left * right;
                    }
                    break;
                case TextRulesHelper.Code_Divide:
                    {
                        var left = children[0].GetValue(variateGet);
                        var right = children[1].GetValue(variateGet);
                        result = left / right;
                    }
                    break;
                case TextRulesHelper.Code_Remainder:
                    {
                        var left = children[0].GetValue(variateGet);
                        var right = children[1].GetValue(variateGet);
                        result = left % right;
                    }
                    break;

            }


            return result;
        }

        /// <summary>
        /// 获取所有叶子节点（即纯变量或纯常量），顺序严格对应文本从左到右
        /// </summary>
        public void GetAllLeafNodes(List<TextRules> resultList)
        {
            // 如果没有子节点，说明自己是底层数据（变量或常量）
            if (children == null || children.Count == 0)
            {
                resultList.Add(this);
                return;
            }

            // 如果有子节点，按顺序遍历（天然等同于从左到右）
            foreach (var child in children)
            {
                child.GetAllLeafNodes(resultList);
            }
        }

        /// <summary>
        /// 自定义解析格式化文本 例:
        /// </summary>
        public string ToString( object variateGet,string format)
        {
            var cache = ListPool<TextRules>.Get();
            GetAllLeafNodes(cache);

            // 2. 
            var result = new StringBuilder();

            int len = format.Length;
            int lastPos = 0;

            for (int i = 0; i < len; i++)
            {
                // 3. 扫描左括号
                if (format[i] == '{')
                {
                    // 把括号之前的纯文本追加进去
                    if (i > lastPos)
                    {
                        result.Append(format, lastPos, i - lastPos);
                    }

                    // 4. 寻找右括号
                    int endIdx = format.IndexOf('}', i);
                    if (endIdx != -1)
                    {
                        // 5. 高效提取括号内的索引（不使用 int.Parse，避免产生垃圾）
                        int index = 0;
                        bool isValidNum = true;
                        for (int j = i + 1; j < endIdx; j++)
                        {
                            char c = format[j];
                            if (c >= '0' && c <= '9')
                            {
                                index = index * 10 + (c - '0');
                            }
                            else
                            {
                                isValidNum = false;
                                break;
                            }
                        }

                        // 6. 如果索引合法，直接把 float 追加进 StringBuilder
                        if (isValidNum && index >= 0 && index < cache.Count)
                        {
                            float val = cache[index].GetValue(variateGet);
                            
                            // 关键点：StringBuilder 原生支持无装箱的 Append(float)
                            // 注：如果想控制小数位数，可以写成 s_SB.Append(val.ToString("F1"))，但 ToString 也会有微量 GC。
                            // 在现代 Unity 中，直接 Append(float) 是最经济的。
                            result.Append(val); 
                        }
                        else
                        {
                            // 如果括号里不是数字（比如非主流文本），原样输出
                            result.Append(format, i, endIdx - i + 1);
                        }

                        // 移动扫描指针
                        i = endIdx;
                        lastPos = i + 1;
                    }
                }
            }

            // 7. 追加尾部剩余的文本
            if (lastPos < len)
            {
                result.Append(format, lastPos, len - lastPos);
            }

            ListPool<TextRules>.Release(cache);

            return result.ToString();
        }

    }
}
