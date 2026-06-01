/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System;

namespace Nodemon.Attributes
{
    /// <summary>
    /// Marks a <c>string</c> field for multi-line TextArea rendering in the
    /// node-properties inspector. <see cref="lines"/> sizes the visible TextArea
    /// (one <c>EditorGUIUtility.singleLineHeight</c> per line); the underlying
    /// string is unbounded — users can scroll inside the TextArea past the
    /// visible window. Same intent as <see cref="UnityEngine.MultilineAttribute"/>
    /// but readable by Nodemon's own GUIProperties drawer so it works for fields
    /// hosted on plain node models (not just MonoBehaviour-style serialization).
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class MultilineAttribute : Attribute
    {
        public int lines;

        public MultilineAttribute(int p_lines = 6)
        {
            lines = p_lines;
        }
    }
}
