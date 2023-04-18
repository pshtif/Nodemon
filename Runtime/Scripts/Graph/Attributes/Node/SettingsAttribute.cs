/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System;

namespace Nodemon.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SettingsAttribute : Attribute
    {
        public bool canHaveMultiple { get; }
        
        public bool enableBaseGUI { get; }
        
        public SettingsAttribute(bool p_canHaveMultiple, bool p_enableBaseGUI)
        {
            canHaveMultiple = p_canHaveMultiple;
            enableBaseGUI = p_enableBaseGUI;
        }
    }
}