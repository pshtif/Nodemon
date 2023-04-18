/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System;

namespace Nodemon
{
    [AttributeUsage(AttributeTargets.Field)]
    public class EditAsStringAttribute : Attribute
    {
        public string ReadMethodName { get; }
        public string WriteMethodName { get; }

        public EditAsStringAttribute(string p_readMethodName, string p_writeMethodName)
        {
            ReadMethodName = p_readMethodName;
            WriteMethodName = p_writeMethodName;
        }
    }
}