/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System;

namespace Nodemon
{
    public static class TypeExtensions
    {
        public static object GetDefaultValue(this Type p_type)
        {
            return (p_type.IsValueType && Nullable.GetUnderlyingType(p_type) == null) ?
                Activator.CreateInstance(p_type) :
                null;
        }
        
        public static string GetReadableTypeName(this Type p_type)
        {
            string typeName = p_type.Name;
            if (p_type.IsGenericType)
            {
                typeName = typeName.Substring(0, typeName.Length - 2) + "<";
                foreach (var type in p_type.GetGenericArguments())
                {
                    typeName += GetReadableTypeName(type) + ",";
                }

                typeName = typeName.Substring(0, typeName.Length - 1) + ">";
            }

            return typeName;
        }

    }
}