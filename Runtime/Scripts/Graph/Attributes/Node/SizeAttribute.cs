/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System;

namespace Nodemon.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SizeAttribute : Attribute
    {
        public int width { get; }
        
        public int height { get; }

        public SizeAttribute(int p_width, int p_height)
        {
            width = p_width;
            height = p_height;
        }
    }
}