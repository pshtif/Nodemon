/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System;

namespace Nodemon.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class CategoryAttribute : Attribute
    {
        public string type { get; }
        
        public string label { get; }

        public CategoryAttribute(string p_name, string p_label = "")
        {
            type = p_name;
            label = p_label;
        }
    }
}