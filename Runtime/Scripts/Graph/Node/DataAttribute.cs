/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Nodemon
{
    public class DataAttribute
    {
        public string name = "";
        public object value;

        public virtual DataAttribute Clone()
        {
            var clone = CreateClonedInstance();
            clone.name = name;
            
            if (value.GetType().IsValueType)
            {
                clone.value = value;
            }

            if (typeof(ICloneable).IsAssignableFrom(value.GetType()))
            {
                clone.value = ((ICloneable)value).Clone();
            }
            else
            {
                if (typeof(IList).IsAssignableFrom(value.GetType()))
                {
                    clone.value = CloneList((IList)value);
                    // clone.value = Enumerable.Range(0, ((IList)value).Count)
                    //     .Select(i => ((IList)value)[i]).ToList();
                }
            }

            return clone;
        }

        protected virtual DataAttribute CreateClonedInstance()
        {
            return new DataAttribute();
        }
        
        public IList CloneList(IList p_list)
        {
            Type elementType = p_list.GetType().GetGenericArguments()[0];
            Type listType = typeof(List<>).MakeGenericType(elementType);

            return (IList)Activator.CreateInstance(listType, new[] { p_list });
        }
    }
}