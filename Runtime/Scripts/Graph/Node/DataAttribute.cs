/*
 *	Created by:  Peter @sHTiF Stefcek
 */

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
            
            if (value.GetType() == typeof(int[]))
            {
                clone.value = ((int[])value).Clone();
            }

            return clone;
        }

        protected virtual DataAttribute CreateClonedInstance()
        {
            return new DataAttribute();
        }
    }
}