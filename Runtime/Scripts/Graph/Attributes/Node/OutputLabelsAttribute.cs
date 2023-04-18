/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System;

namespace Nodemon.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class OutputLabelsAttribute : Attribute
    {
        public string[] labels { get; }

        public OutputLabelsAttribute(params string[] p_labels)
        {
            labels = p_labels;
        }
    }
}