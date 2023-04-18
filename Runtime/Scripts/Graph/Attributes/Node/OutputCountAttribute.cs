/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System;

namespace Nodemon.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class OutputCountAttribute : Attribute
    {
        public int count = 0;

        public OutputCountAttribute(int p_count)
        {
            count = p_count;
        }
    }
}