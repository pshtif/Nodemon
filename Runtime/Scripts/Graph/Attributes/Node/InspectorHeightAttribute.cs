/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System;

namespace Nodemon.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class InspectorHeightAttribute : Attribute
    {
        public int height { get; }

        public InspectorHeightAttribute(int p_height)
        {
            height = p_height;
        }
    }
}