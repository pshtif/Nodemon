/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System;

namespace Nodemon.Attributes
{
    /// <summary>
    /// Shows a field only while another field on the same model holds one of the
    /// given values — for nodes that carry several modes' worth of parameters and
    /// should only ever present one mode's worth.
    ///
    /// The controlling field must be an ENUM parameter, and the values are its
    /// integer values. Anything else has no stable identity to compare against.
    ///
    /// A field with no attribute is always visible, so this is opt-in and nothing
    /// that existed before it changes.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class VisibleWhenAttribute : Attribute
    {
        public string Controller { get; }

        public int[] Values { get; }

        public VisibleWhenAttribute(string p_controller, params int[] p_values)
        {
            Controller = p_controller;
            Values = p_values;
        }
    }
}
