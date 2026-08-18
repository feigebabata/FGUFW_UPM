// using System;
// using System.Collections.Generic;
// using System.Text;

// namespace FGUFW
// {
//     /// <summary>
//     /// 位枚举数组 枚举按位赋值 必须>0
//     /// </summary>
//     public struct Bit64Enums<E> where E:Enum
//     {
//         private long _bits;

//         public long Value => _bits;

//         public bool this[int i]
//         {
//             get
//             {
//                 if(i==0)return false;

//                 return Bit64Utility.Contains(_bits,i);
//             }
//             set
//             {
//                 if(i==0)return;

//                 if(value)
//                 {
//                     _bits = Bit64Utility.Add(_bits,i);
//                 }
//                 else
//                 {
//                     _bits = Bit64Utility.Sub(_bits,i);
//                 }
//             }
//         }



//         public Bit64Enums(long bits)
//         {
//             _bits = bits;
//         }

//         public BitEnums<E> Pares(string text,char separator=',')
//         {
//             var bitEnums = new BitEnums<E>();
//             var items = text.Split(separator);
//             foreach (var item in items)
//             {
//                 var i = item.ToEnum<E>();
//                 bitEnums[i]=true;
//             }
//             return bitEnums;
//         }

//         public void Clear()
//         {
//             _bits = 0;
//         }

//         public override string ToString()
//         {
//             if(_bits==0)return "0";
//             StringBuilder sb = new StringBuilder();
//             sb.AppendLine(_bits.ToString());
//             sb.AppendLine(_bits.ToBitString());
//             return sb.ToString();
//         }

//     }
// }
