/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System;

namespace Nodemon.Attributes
{
    /// <summary>
    /// Attaches Markdown-formatted help to a node class. Rendered by the
    /// host's help panel (Machina's HelpView) via the
    /// <c>UniversalGUI.UniGUIMarkdown</c> renderer when the node is selected.
    ///
    /// <para>Short inline help — a paragraph or two with examples. For
    /// anything longer than a screenful, consider pointing to an external
    /// topic doc from inside the markdown rather than inlining the whole
    /// thing. The attribute value is a verbatim string; use C# multiline
    /// raw-string syntax (<c>@""</c>) so you can write actual Markdown
    /// without escape pain.</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class HelpAttribute : Attribute
    {
        public string Markdown { get; }

        public HelpAttribute(string p_markdown)
        {
            Markdown = p_markdown;
        }
    }
}
