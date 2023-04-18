/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System;

namespace Nodemon.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ContextNameAttribute : Attribute
    {
        public string name { get; }
        
        public ContextNameAttribute(string p_name)
        {
            name = p_name;
        }
    }
}