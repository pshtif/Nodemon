/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System;

namespace Nodemon.Attributes
{
    /// <summary>
    /// The exact text to show for a field, instead of one derived from its name.
    ///
    /// <para>By default the inspector builds a label by splitting the field name on capitals
    /// — <c>segmentDistribution</c> becomes "Segment Distribution". That is right for a name
    /// written as an identifier, and wrong for one that is already prose: it double-spaces
    /// "Segment Distribution" and turns "FPS" into "F P S".</para>
    ///
    /// <para>Use it where the label is authored rather than derived — an acronym, a
    /// punctuated name, or a name that comes from somewhere outside C#.</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class LabelAttribute : Attribute
    {
        public readonly string label;

        public LabelAttribute(string p_label)
        {
            label = p_label;
        }
    }
}
