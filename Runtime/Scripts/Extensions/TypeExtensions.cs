/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System;
using System.Reflection;

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
        
        public static bool IsSubclassOfGeneric(this Type p_type, Type p_generic) {
            while (p_type != null && p_type != typeof(object) && !p_type.IsInterface) {
                var cur = p_type.IsGenericType ? p_type.GetGenericTypeDefinition() : p_type;
                if (p_generic == cur) {
                    return true;
                }
                p_type = p_type.BaseType;
            }
            return false;
        }

        public static bool IsImplicitlyAssignableFrom(this Type p_source, Type p_target)
        {
            var sourceCode = Type.GetTypeCode( p_source );
            var targetCode = Type.GetTypeCode( p_target );
            
            switch( sourceCode )
            {
                case TypeCode.SByte:
                    switch( targetCode )
                    {
                        case TypeCode.Int16:
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    return false;
                case TypeCode.Byte:
                    switch( targetCode )
                    {
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    return false;
                case TypeCode.Int16:
                    switch( targetCode )
                    {
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    return false;
                case TypeCode.UInt16:
                    switch( targetCode )
                    {
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    return false;
                case TypeCode.Int32:
                    switch( targetCode )
                    {
                        case TypeCode.Int64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    return false;
                case TypeCode.UInt32:
                    switch( targetCode )
                    {
                        case TypeCode.UInt32:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    return false;
                case TypeCode.Int64:
                case TypeCode.UInt64:
                    switch( targetCode )
                    {
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    return false;
                case TypeCode.Char:
                    switch( targetCode )
                    {
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    return false;
                case TypeCode.Single:
                    return ( targetCode == TypeCode.Double );
            }
            return false;
        }
    }
}