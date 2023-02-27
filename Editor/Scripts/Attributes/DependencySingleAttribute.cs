/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System;

namespace Nodemon.Attributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class DependencySingleAttribute : Attribute
    {
        public object Value { get; }

        public string DependencyName { get; }

        public DependencySingleAttribute(string p_dependencyName, object p_value)
        {
            DependencyName = p_dependencyName;
            Value = p_value;
        }
    }
}