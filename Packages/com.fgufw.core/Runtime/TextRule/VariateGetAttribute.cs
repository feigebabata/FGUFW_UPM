using System;

namespace FGUFW
{
    [AttributeUsage(AttributeTargets.Field|AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class VariateGetAttribute : Attribute
    {
        public readonly string Alias;

        public VariateGetAttribute(string alias=default)
        {
            Alias = alias;
        }

    }




}