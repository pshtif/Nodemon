/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System;

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

            return clone;
        }

        protected virtual DataAttribute CreateClonedInstance()
        {
            return new DataAttribute();
        }
    }
}