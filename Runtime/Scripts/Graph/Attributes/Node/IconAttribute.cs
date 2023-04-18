/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System;

namespace Nodemon.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class IconAttribute : Attribute
    {
        public string iconId { get; }
        
        public IconAttribute(string p_iconId)
        {
            iconId = p_iconId;
        }
    }
}