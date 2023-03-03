/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System.Collections.Generic;
using UnityEngine;

namespace Nodemon
{
    public static class UniversalGUI
    {
        static public GUISkin Skin => (GUISkin)Resources.Load("RuntimeSkin");
        
        private static readonly Stack<bool> _changedStack = new Stack<bool>();
        
        public static bool changed = false;

        public static void BeginChangeCheck()
        {
            #if UNITY_EDITOR
            UnityEditor.EditorGUI.BeginChangeCheck();
            #endif
            _changedStack.Push(changed);
            changed = false;
        }

        public static bool EndChangeCheck()
        {
            if (_changedStack.Count == 0)
            {
                changed = true;
                return true;
            }

            bool currentChanged = changed;
            #if UNITY_EDITOR
            currentChanged |= UnityEditor.EditorGUI.EndChangeCheck();
            #endif
            changed |= _changedStack.Pop();
            return currentChanged;
        }

        public static string NicifyString(string p_string)
        {
            string nicifed = ""; 
            for(int i = 0; i < p_string.Length; i++)
            {
                if (p_string[i] == '_')
                    continue;

                if(char.IsUpper(p_string[i]) && i != 0)
                {
                    nicifed += " ";
                }
                
                nicifed += p_string[i];
            }

            return nicifed;
        }

        public static void Box(Rect p_rect, Color p_color)
        {
            GUI.Box(p_rect, TextureUtils.GetColorTexture(p_color));
        }
        
        public static bool Toggle(Rect p_rect, bool p_value)
        {
            int thisId = GUIUtility.GetControlID("Toggle".GetHashCode(), FocusType.Keyboard);
            Event current = Event.current;
            EventType type = current.type;
            bool flag1 = current.type == EventType.MouseDown && current.button != 0;
            if (flag1)
            {
                current.type = EventType.Ignore;
            }

            var boolValue = GUI.Toggle(p_rect, (bool) p_value, "", Skin.toggle);

            if (flag1)
                current.type = type;
            else if (current.type != type)
                GUIUtility.keyboardControl = thisId;

            if (boolValue != p_value)
            {
                UniversalGUI.changed = true;
            }

            return boolValue;
        }
    }
}