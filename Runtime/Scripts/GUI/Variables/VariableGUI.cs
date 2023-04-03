/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using UnityEngine;

namespace Nodemon
{
    public class VariableGUI
    {
        public static bool DrawVariablesInspector(string p_title, Variables p_variables, IVariableBindable p_bindable, float p_maxWidth, ref bool p_minimized)
        {
            if (!DrawMinimizableTitle(p_title, ref p_minimized))
                return false;

            return DrawVariablesInspector("", p_variables, p_bindable, p_maxWidth);
        }
        
        public static bool DrawVariablesInspector(string p_title, Variables p_variables, IVariableBindable p_bindable, float p_maxWidth)
        {
            DrawTitle(p_title, 13);

            int index = 0;
            bool invalidate = false;
            p_variables._variables?.ForEach(variable =>
            {
                invalidate = invalidate || VariableField(p_variables, variable.Name, p_bindable,
                    p_maxWidth);
                GUILayout.Space(2);
                index++;
            });
            
            GUILayout.Space(4);
            
            GUI.color = new Color(1, .5f, 0);
            if (UniversalGUILayout.Button("Add Variable", GUILayout.Height(24)))
            {
                invalidate = true;
#if UNITY_EDITOR
                VariableTypesMenu.Show(p_variables, Event.current.mousePosition);
#else
                VariableTypesMenu.Show(p_variables, GUIUtility.GUIToScreenPoint(Event.current.mousePosition));
#endif
            }
            GUI.color = Color.white;

            return invalidate;
        }

        public static bool VariableField(Variables p_variables, string p_name, IVariableBindable p_bindable, float p_maxWidth)
        {
            bool invalidate = false;
            var variable = p_variables.GetVariable(p_name);
            GUILayout.BeginHorizontal();
            string newName = UniversalGUILayout.TextField(p_name, GUILayout.Width(120));
            GUILayout.Space(2);
            if (newName != p_name)
            {
                invalidate = true;   
                p_variables.RenameVariable(p_name, newName);
            }
            
            invalidate = invalidate || variable.ValueField(p_maxWidth-150, p_bindable);

            var oldColor = GUI.color;
            GUI.color = variable.IsBound || variable.IsLookup ? Color.yellow : Color.gray;
            
            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical(GUILayout.Width(16));
            GUILayout.Space(2);
            if (GUILayout.Button(TextureUtils.GetTexture("Icons/settings_icon"), GUIStyle.none, GUILayout.Height(16), GUILayout.Width(16)))
            {
                var menu = VariableSettingsMenu.Get(p_variables, p_name, p_bindable);
                GenericMenuPopup.Show(menu, "", Event.current.mousePosition, 240, 300, false, false);
            }
            GUILayout.EndVertical();

            GUI.color = oldColor;

            GUILayout.EndHorizontal();

            return invalidate;
        }
        
        public static bool DrawMinimizableTitle(string p_title, ref bool p_minimized, int? p_size = null, Color? p_color = null, TextAnchor? p_alignment = null, int p_rightOffset = 0)
        {
            var style = new GUIStyle();
            style.normal.textColor = p_color.HasValue ? p_color.Value : Color.white;
            style.alignment = p_alignment.HasValue ? p_alignment.Value : TextAnchor.MiddleCenter;
            style.fontStyle = FontStyle.Bold;
            style.normal.background = Texture2D.whiteTexture;
            style.fontSize = p_size.HasValue ? p_size.Value : 13;
            GUI.backgroundColor = new Color(0, 0, 0, .5f);
            GUILayout.Label(p_title, style, GUILayout.Height(style.fontSize * 2));
            GUI.backgroundColor = Color.white;
            
            var rect = GUILayoutUtility.GetLastRect();

            style = new GUIStyle();
            style.fontSize = p_size.HasValue ? p_size.Value + 6 : 20;
            style.normal.textColor = p_color.HasValue
                ? p_color.Value * 2f / 3
                : Color.white * 2f / 3;

            GUI.Label(new Rect(rect.x + 6 + (p_minimized ? 0 : 2), rect.y, 24, 24), p_minimized ? "+" : "-", style);
            
            if (GUI.Button(new Rect(rect.x, rect.y, rect.width - p_rightOffset, rect.height), "", GUIStyle.none))
            {
                p_minimized = !p_minimized;
            }

            return !p_minimized;
        }
        
        public static void DrawTitle(string p_title, int p_size)
        {
            var titleStyle = new GUIStyle();
            titleStyle.alignment = TextAnchor.MiddleCenter;
            titleStyle.normal.textColor = Color.white;
            titleStyle.normal.background = Texture2D.whiteTexture;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.fontSize = p_size;

            GUI.backgroundColor = new Color(0, 0, 0, .5f);

            GUILayout.Label(p_title, titleStyle, GUILayout.ExpandWidth(true), GUILayout.Height(p_size * 2));
            GUILayout.Space(4);

            GUI.backgroundColor = Color.white;
        }
    }
}