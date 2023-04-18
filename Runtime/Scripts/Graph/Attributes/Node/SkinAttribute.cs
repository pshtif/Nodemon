/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System;

namespace Nodemon.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SkinAttribute : Attribute
    {
        public string titleSkinId { get; }
        public string backgroundSkinId { get; }
        
        public SkinAttribute(string p_titleSkinId, string p_backgroundSkinId)
        {
            titleSkinId = p_titleSkinId;
            backgroundSkinId = p_backgroundSkinId;
        }
    }
}