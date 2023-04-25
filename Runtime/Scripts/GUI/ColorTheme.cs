/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using UnityEngine;

namespace Machina
{
    // TODO move this to scriptable object later
    public static class ColorTheme
    {
        public static Color BUTTON_COLOR = new Color(1, 0.75f, 0.5f);
        
        public static Color NODE_PASS_ACTIVE_COLOR = new Color(1, 1, .1f);
        public static Color NODE_PASS_INACTIVE_COLOR = new Color(.8f, 0.8f, .8f);
        
        public static Color NODE_TEMPLATE_ACTIVE_COLOR = new Color(1, .1f, 1f);
        public static Color NODE_TEMPLATE_INACTIVE_COLOR = new Color(.8f, 0.8f, .8f);
        
        public static Color NODE_OUTPUT_ACTIVE_COLOR = new Color(0, .6f, 1f);
        public static Color NODE_OUTPUT_INACTIVE_COLOR = new Color(0.8f, .8f, .8f);
    }
}