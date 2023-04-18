/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System;

namespace Nodemon
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class DependencyAttribute : Attribute
    {
        public object Value { get; }

        public string DependencyName { get; }

        public DependencyAttribute(string p_dependencyName, object p_value)
        {
            DependencyName = p_dependencyName;
            Value = p_value;
        }
    }
}