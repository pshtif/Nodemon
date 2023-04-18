/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System;

namespace Nodemon.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class InputCountAttribute : Attribute
    {
        public int count = 0;

        public InputCountAttribute(int p_count)
        {
            count = p_count;
        }
    }
}