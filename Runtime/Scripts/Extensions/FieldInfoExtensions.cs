/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System.Reflection;

namespace Nodemon
{
    public static class FieldInfoExtensions
    {
        public static bool IsConstant(this FieldInfo field)
        {
            return field.IsReadOnly() && field.IsStatic;
        }
        
        public static bool IsReadOnly(this FieldInfo field) 
        {
            return field.IsInitOnly || field.IsLiteral;
        }
    }
}