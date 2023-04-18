/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System;

namespace Nodemon.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class InputAllowMultipleAttribute : Attribute
    {
        public bool[] allowMultiple;

        public InputAllowMultipleAttribute(params bool[] p_allowMultiple)
        {
            allowMultiple = p_allowMultiple;
        }
    }
}