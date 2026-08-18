#region Header
/**
 * JsonWriter.cs
 * 类似流的 facility，用于输出 JSON 文本。
 *
 * 作者声明放弃对此源代码的版权。有关详细信息，请参阅
 * 此发行版中包含的 COPYING 文件。
 **/
#endregion


using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;


namespace LitJson
{
    /// <summary>
    /// 内部状态枚举：用于验证当前写入操作是否合法
    /// </summary>
    internal enum Condition
    {
        InArray,      // 在数组中
        InObject,     // 在对象中
        NotAProperty, // 不是属性名位置
        Property,     // 属性名位置
        Value         // 值位置
    }

    /// <summary>
    /// 写入上下文：记录当前的嵌套层级状态
    /// </summary>
    internal class WriterContext
    {
        public int Count;          // 当前层级已写入的元素数量
        public bool InArray;        // 是否在数组内
        public bool InObject;       // 是否在对象内
        public bool ExpectingValue; // 是否正在等待写入值（在属性名之后）
    }

    public class JsonWriter
    {
        #region 字段 (Fields)
        private static readonly NumberFormatInfo number_format; // 数字格式化定义

        private WriterContext context;             // 当前上下文
        private Stack<WriterContext> ctx_stack;           // 上下文堆栈，用于处理嵌套结构
        private bool has_reached_end;     // 是否已完成整个 JSON 结构的写入
        private char[] hex_seq;             // 用于处理 Unicode 转义的临时字符数组
        private int indentation;         // 当前累计的缩进空格数
        private int indent_value;        // 每一层级增加的缩进空格数
        private StringBuilder inst_string_builder; // 如果使用默认构造函数，内部使用的 StringBuilder
        private bool pretty_print;        // 是否启用格式化输出（换行与缩进）
        private bool validate;            // 是否执行 JSON 语法合法性验证
        private bool lower_case_properties; // 是否强制将属性名转为小写
        private TextWriter writer;              // 实际执行写入的目标流
        #endregion


        #region 属性 (Properties)
        /// <summary>
        /// 获取或设置每层缩进的空格数
        /// </summary>
        public int IndentValue
        {
            get { return indent_value; }
            set
            {
                indentation = (indentation / indent_value) * value;
                indent_value = value;
            }
        }

        /// <summary>
        /// 是否开启漂亮打印（格式化输出）
        /// </summary>
        public bool PrettyPrint
        {
            get { return pretty_print; }
            set { pretty_print = value; }
        }

        /// <summary>
        /// 获取当前使用的 TextWriter
        /// </summary>
        public TextWriter TextWriter
        {
            get { return writer; }
        }

        /// <summary>
        /// 是否验证 JSON 结构的完整性和合法性
        /// </summary>
        public bool Validate
        {
            get { return validate; }
            set { validate = value; }
        }

        /// <summary>
        /// 是否自动将属性名转换为小写
        /// </summary>
        public bool LowerCaseProperties
        {
            get { return lower_case_properties; }
            set { lower_case_properties = value; }
        }
        #endregion


        #region 构造函数 (Constructors)
        static JsonWriter()
        {
            number_format = NumberFormatInfo.InvariantInfo;
        }

        public JsonWriter()
        {
            inst_string_builder = new StringBuilder();
            writer = new StringWriter(inst_string_builder);

            Init();
        }

        public JsonWriter(StringBuilder sb) :
            this(new StringWriter(sb))
        {
        }

        public JsonWriter(TextWriter writer)
        {
            if (writer == null)throw new ArgumentNullException("writer");

            this.writer = writer;

            Init();
        }
        #endregion


        #region 私有方法 (Private Methods)
        /// <summary>
        /// 状态合法性检查：确保在正确的位置写入正确的符号
        /// </summary>
        private void DoValidation(Condition cond)
        {
            if (!context.ExpectingValue) context.Count++;

            if (!validate) return;

            if (has_reached_end)
                throw new JsonException(
                    "完整的 JSON 结构已经结束，无法继续写入");

            switch (cond)
            {
                case Condition.InArray:
                    if (!context.InArray)
                        throw new JsonException("当前不在数组中，无法结束数组");
                    break;

                case Condition.InObject:
                    if (!context.InObject || context.ExpectingValue)
                        throw new JsonException("当前状态无法结束对象");
                    break;

                case Condition.NotAProperty:
                    if (context.InObject && !context.ExpectingValue)
                        throw new JsonException("预期需要一个属性名");
                    break;

                case Condition.Property:
                    if (!context.InObject || context.ExpectingValue)
                        throw new JsonException("当前状态无法添加属性名");
                    break;

                case Condition.Value:
                    if (!context.InArray &&
                        (!context.InObject || !context.ExpectingValue))
                        throw new JsonException("当前状态无法添加值");

                    break;
            }
        }

        /// <summary>
        /// 初始化写入器状态
        /// </summary>
        private void Init()
        {
            has_reached_end = false;
            hex_seq = new char[4];
            indentation = 0;
            indent_value = 4;
            pretty_print = false;
            validate = true;
            lower_case_properties = false;

            ctx_stack = new Stack<WriterContext>();
            context = new WriterContext();
            ctx_stack.Push(context);
        }

        /// <summary>
        /// 将整数转换为 4 位十六进制字符，用于 Unicode 转义
        /// </summary>
        private static void IntToHex(int n, char[] hex)
        {
            int num;

            for (int i = 0; i < 4; i++)
            {
                num = n % 16;

                if (num < 10)
                    hex[3 - i] = (char)('0' + num);
                else
                    hex[3 - i] = (char)('A' + (num - 10));

                n >>= 4;
            }
        }

        /// <summary>
        /// 增加缩进层级
        /// </summary>
        private void Indent()
        {
            if (pretty_print) indentation += indent_value;
        }


        /// <summary>
        /// 基础写入方法：根据 pretty_print 状态决定是否先写入缩进空格
        /// </summary>
        private void Put(string str)
        {
            if (pretty_print && !context.ExpectingValue)
            {
                PutSpace();
            }

            writer.Write(str);
        }

        private void PutSpace()
        {
            for (int i = 0; i < indentation; i++)
            {
                writer.Write(' ');
            }
        }

        /// <summary>
        /// 写入换行符
        /// </summary>
        private void PutNewline()
        {
            PutNewline(true);
        }

        /// <summary>
        /// 写入逗号（根据需要）和换行符
        /// </summary>
        /// <param name="add_comma">是否尝试添加逗号</param>
        private void PutNewline(bool add_comma)
        {
            if (add_comma && !context.ExpectingValue && context.Count > 1)
            {
                writer.Write(',');
            }

            if (pretty_print && !context.ExpectingValue)
            {
                writer.Write(Environment.NewLine);
            }
        }

        /// <summary>
        /// 写入转义后的字符串内容
        /// </summary>
        private void PutString(string str)
        {
            Put(String.Empty); // 处理缩进

            writer.Write('"');

            int n = str.Length;
            for (int i = 0; i < n; i++)
            {
                switch (str[i])
                {
                    case '\n': writer.Write("\\n"); continue;
                    case '\r': writer.Write("\\r"); continue;
                    case '\t': writer.Write("\\t"); continue;
                    case '"':
                    case '\\':
                        {
                            writer.Write('\\');
                            writer.Write(str[i]);
                            continue;
                        }
                    case '\f': writer.Write("\\f"); continue;
                    case '\b': writer.Write("\\b"); continue;
                }

                if ((int)str[i] >= 32 && (int)str[i] <= 126)
                {
                    writer.Write(str[i]);
                    continue;
                }

                // 处理非 ASCII 字符，转为 \uXXXX
                IntToHex((int)str[i], hex_seq);
                writer.Write("\\u");
                writer.Write(hex_seq);
            }

            writer.Write('"');
        }

        /// <summary>
        /// 减少缩进层级
        /// </summary>
        private void Unindent()
        {
            if (pretty_print) indentation -= indent_value;
        }
        #endregion


        public override string ToString()
        {
            if (inst_string_builder == null) return String.Empty;

            return inst_string_builder.ToString();
        }

        /// <summary>
        /// 重置写入器状态，以便重新使用
        /// </summary>
        public void Reset()
        {
            has_reached_end = false;

            ctx_stack.Clear();
            context = new WriterContext();
            ctx_stack.Push(context);

            if (inst_string_builder != null) inst_string_builder.Remove(0, inst_string_builder.Length);
        }

        #region 写入值的方法 (Write Methods for Values)
        public void Write(bool boolean)
        {
            DoValidation(Condition.Value);
            PutNewline();
            Put(boolean ? "true" : "false");
            context.ExpectingValue = false;
        }

        public void Write(decimal number)
        {
            DoValidation(Condition.Value);
            PutNewline();
            Put(Convert.ToString(number, number_format));
            context.ExpectingValue = false;
        }

        public void Write(float number)
        {
            DoValidation(Condition.Value);
            PutNewline();

            string str = Convert.ToString(number, number_format);
            Put(str);

            if (str.IndexOf('.') == -1 && str.IndexOf('E') == -1)
                writer.Write(".0");

            context.ExpectingValue = false;
        }

        public void Write(int number)
        {
            DoValidation(Condition.Value);
            PutNewline();
            Put(Convert.ToString(number, number_format));
            context.ExpectingValue = false;
        }

        public void Write(long number)
        {
            DoValidation(Condition.Value);
            PutNewline();
            Put(Convert.ToString(number, number_format));
            context.ExpectingValue = false;
        }

        public void Write(string str)
        {
            DoValidation(Condition.Value);
            PutNewline();

            if (str == null)
                Put("null");
            else
                PutString(str);

            context.ExpectingValue = false;
        }

        [CLSCompliant(false)]
        public void Write(ulong number)
        {
            DoValidation(Condition.Value);
            PutNewline();
            Put(Convert.ToString(number, number_format));
            context.ExpectingValue = false;
        }
        #endregion

        /// <summary>
        /// 开始一个新数组
        /// </summary>
        public void WriteArrayStart()
        {
            DoValidation(Condition.NotAProperty);
            // PutNewline();
            
            if(pretty_print)
            {
                writer.Write(Environment.NewLine);
                PutSpace();
            }

            Put("[");

            context = new WriterContext();
            context.InArray = true;
            ctx_stack.Push(context);

            Indent();
        }

        /// <summary>
        /// 结束当前数组
        /// </summary>
        public void WriteArrayEnd()
        {
            DoValidation(Condition.InArray);
            PutNewline(false); // 闭合括号前的换行

            ctx_stack.Pop();
            if (ctx_stack.Count == 1)
                has_reached_end = true;
            else
            {
                context = ctx_stack.Peek();
                context.ExpectingValue = false;
            }

            Unindent();
            Put("]");
        }

        /// <summary>
        /// 开始一个新对象
        /// </summary>
        public void WriteObjectStart()
        {
            DoValidation(Condition.NotAProperty);
            // PutNewline();

            if(pretty_print)
            {
                writer.Write(Environment.NewLine);
                PutSpace();
            }

            Put("{");

            context = new WriterContext();
            context.InObject = true;
            ctx_stack.Push(context);

            Indent();
        }

        /// <summary>
        /// 结束当前对象
        /// </summary>
        public void WriteObjectEnd()
        {
            DoValidation(Condition.InObject);
            PutNewline(false); // 闭合括号前的换行

            ctx_stack.Pop();
            if (ctx_stack.Count == 1)
                has_reached_end = true;
            else
            {
                context = ctx_stack.Peek();
                context.ExpectingValue = false;
            }

            Unindent();
            Put("}");
        }

        /// <summary>
        /// 写入属性名（Key）
        /// </summary>
        public void WritePropertyName(string property_name)
        {
            DoValidation(Condition.Property);
            PutNewline();
            string propertyName = (property_name == null || !lower_case_properties)
                ? property_name
                : property_name.ToLowerInvariant();

            PutString(propertyName);

            writer.Write(":");

            context.ExpectingValue = true;
        }
    }
}