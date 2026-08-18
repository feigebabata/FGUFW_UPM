using System;
using System.Collections.Generic;
using System.Reflection;

namespace FGUFW
{
    
    public static class VariateGetUtility
    {
        
        static Dictionary<Type,List<MemberInfo>> variateMembers = new Dictionary<Type, List<MemberInfo>>();

        public static List<MemberInfo> GetVariateMembers(Type type)
        {
            if(variateMembers.ContainsKey(type))
            {
                return variateMembers[type];
            }
            var members = new List<MemberInfo>();
            variateMembers.Add(type,members);
            
            foreach (FieldInfo f_info in type.GetFieldsByCache())
            {
                if(f_info.GetCustomAttribute<VariateGetAttribute>()!=default)
                {
                    if(f_info.FieldType != typeof(float))
                    {
                        throw new Exception($"VariateGetAttribute fieldType not float :{f_info.Name} ");
                    }
                    members.Add(f_info);
                }
            }

            foreach (PropertyInfo p_info in type.GetProperties (BindingFlags.Instance | BindingFlags.Public  | BindingFlags.NonPublic))
            {
                if(p_info.GetCustomAttribute<VariateGetAttribute>()!=default)
                {
                    if(p_info.PropertyType != typeof(float))
                    {
                        throw new Exception($"VariateGetAttribute propertyType not float :{p_info.Name} ");
                    }
                    members.Add(p_info);
                }
            }

            return members;
        }

        public static float GetValue(object variateGet,bool variate,float val)
        {
            if(variate)
            {
                var members = GetVariateMembers(variateGet.GetType());
                var member = members[(int)val];

                if(member is FieldInfo)
                {
                    var field = member as FieldInfo;
                    return (float)field.GetValue(variateGet);
                }
                else
                {
                    var property = member as PropertyInfo;
                    return (float)property.GetValue(variateGet);
                }

            }
            else
            {
                return val;
            }
        }

        public static float GetVariateKey(object variateGet,string name)
        {
            var members = GetVariateMembers(variateGet.GetType());
            for (int i = 0; i < members.Count; i++)
            {
                var memberr = members[i];
                var varAtt = memberr.GetCustomAttribute<VariateGetAttribute>();
                string key = default;
                if(varAtt.Alias.IsNull())
                {
                    key = memberr.Name;
                }
                else
                {
                    key = varAtt.Alias;
                }

                if(key==name)
                {
                    return i;
                }
            }

            throw new Exception($"GetVariateKey unknown key :{name} ");
        }

    }


}
