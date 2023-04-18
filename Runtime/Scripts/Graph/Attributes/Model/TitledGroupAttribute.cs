/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System;

namespace Nodemon.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class TitledGroupAttribute : Attribute
    {
        public string Group { get; }
        
        public bool Minimized { get; set; }
        
        public int Order { get; }

        public TitledGroupAttribute(string p_group, int p_order = 0, bool p_minized = false)
        {
            Group = p_group;
            Order = p_order;
            Minimized = p_minized;
        }
    }
}