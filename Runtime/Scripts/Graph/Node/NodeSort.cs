/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System.Reflection;
using Nodemon.Attributes;

namespace Nodemon
{
    public class NodeSort
    {
        static public int GroupSort(FieldInfo p_field1, FieldInfo p_field2)
        {
            TitledGroupAttribute attribute1 = p_field1.GetCustomAttribute<TitledGroupAttribute>();
            TitledGroupAttribute attribute2 = p_field2.GetCustomAttribute<TitledGroupAttribute>();
            if (attribute1 == null && attribute2 == null)
                return OrderSort(p_field1, p_field2);

            if (attribute1 != null && attribute2 == null)
                return 1;

            if (attribute1 == null && attribute2 != null)
                return -1;

            if (attribute1.Group == attribute2.Group)
                return OrderSort(p_field1, p_field2);
            
            if (attribute1.Order != attribute2.Order) 
                return attribute1.Order.CompareTo(attribute2.Order);
            
            return attribute1.Group.CompareTo(attribute2.Group);
        }
        
        static public int OrderSort(FieldInfo p_field1, FieldInfo p_field2)
        {
            OrderAttribute attribute1 = p_field1.GetCustomAttribute<OrderAttribute>();
            OrderAttribute attribute2 = p_field2.GetCustomAttribute<OrderAttribute>();
            
            if (attribute1 == null && attribute2 == null)
                return 0;

            if (attribute1 != null && attribute2 == null)
                return -1;
            
            if (attribute1 == null && attribute2 != null)
                return 1;

            return attribute1.Order.CompareTo(attribute2.Order);
        }
    }
}