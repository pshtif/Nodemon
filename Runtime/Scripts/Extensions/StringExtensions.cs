/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System;
using System.Linq;

namespace Nodemon
{
    public static class StringExtensions
    {
        public static string RemoveWhitespace(this string p_input)
        {
            if (p_input == null)
                return null;
            
            return new string(p_input.Where(c => !Char.IsWhiteSpace(c)).ToArray());
        }
    }
}