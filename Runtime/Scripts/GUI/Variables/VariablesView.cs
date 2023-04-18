/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System;
using UnityEngine;
using UniversalGUI;

namespace Nodemon
{
    public class VariablesView : ViewBase
    {
        private Vector2 scrollPosition;

        public override void DrawGUI(Event p_event, Rect p_rect)
        {
            
        }
        
        protected void DrawVariablesGUI(Vector2 p_position, bool p_global, Color p_color, Variables p_variables, ref bool p_minimized, IVariableBindable p_bindable) 
        {
            Rect rect = new Rect(p_position.x, p_position.y, 380, p_minimized ? 32 : 200);
            DrawBoxGUI(rect, p_global ? "Global Variables" : "Graph Variables", TextAnchor.UpperCenter, p_color);

            var minStyle = new GUIStyle();
            minStyle.normal.textColor = Color.white;
            minStyle.fontStyle = FontStyle.Bold;
            minStyle.fontSize = 20;
            if (GUI.Button(new Rect(rect.x + rect.width - 20 + (p_minimized ? 0 : 2), rect.y + 2, 20, 20), p_minimized ? "+" : "-", minStyle))
            {
                p_minimized = !p_minimized;
                GUI.FocusControl("");
            }

            if (p_minimized)
                return;

            #if UNITY_EDITOR
            if (p_global && p_bindable != null && UnityEditor.PrefabUtility.GetPrefabInstanceStatus(p_bindable.gameObject) != UnityEditor.PrefabInstanceStatus.NotAPrefab)
            {
                var style = new GUIStyle();
                style.alignment = TextAnchor.MiddleCenter;
                style.normal.textColor = Color.white;
                style.fontSize = 20;
                style.wordWrap = true;
                UnityEditor.EditorGUI.TextArea(new Rect(rect.x+5, rect.y+30, rect.width-10, rect.height-30),"Global variables on prefab instances are not supported!", style);
                return;
            }
            #endif

            GUILayout.BeginArea(new Rect(rect.x+5, rect.y+30, rect.width-10, rect.height-79));
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, false);

            UniGUI.BeginChangeCheck();

            if (p_variables != null)
            {
                int index = 0;
                foreach (var variable in p_variables.Get())
                {
                    if (variable == null)
                    {
                        p_variables.Get().Remove(variable);
                        break;
                    }
                    //var r = new Rect(0, 25 + 24 * index, rect.width, 30);
                    VariableGUI.VariableField(p_variables, variable.Name, p_bindable, rect.width - 10);
                    UniGUILayout.Space(4);
                    index++;
                }
            }

            GUILayout.EndScrollView();
            GUILayout.EndArea();

            if (GUI.Button(new Rect(rect.x + 4, rect.y + rect.height - 48, rect.width - 8, 20), "Add Variable"))
            {
                Owner.TypesMenu.Show((type) => OnAddVariable(p_variables, type));
            }
            
            if (GUI.Button(new Rect(rect.x + 4, rect.y + rect.height - 24, rect.width/2-6, 20), "Copy Variables"))
            {
                VariableUtils.CopyVariables(p_variables);
            }
            
            if (GUI.Button(new Rect(rect.x + rect.width/2 + 2, rect.y + rect.height - 24, rect.width/2-6, 20), "Paste Variables"))
            {
                VariableUtils.PasteVariables(p_variables, p_bindable);
            }
            
            if (UniGUI.EndChangeCheck())
            {
                Graph.MarkDirty();
            }

            UseEvent(new Rect(rect.x, rect.y, rect.width, rect.height));
        }

        void OnAddVariable(Variables p_variables, Type p_type)
        {
            string name = "new"+p_type.ToString().Substring(p_type.ToString().LastIndexOf(".")+1);

            int index = 0;
            while (p_variables.HasVariable(name + index)) index++;
            
            p_variables.AddVariableByType((Type)p_type, name+index, null);
            
            Graph.MarkDirty();
        }
    }
}