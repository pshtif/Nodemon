/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System;
using System.Collections.Generic;
using System.Reflection;
using Nodemon.Attributes;
using UnityEngine;
using UniversalGUI;
using GUI = UnityEngine.GUI;

namespace Nodemon
{
    [Serializable]
    public class GraphBox
    {
        public static GraphBox editingBoxComment;
        public static GraphBox selectedBox;
        
        public string comment;

        public Color color = Color.white;

        public bool moveNodes = true;
        
        [Hide]
        public Rect rect;

        public Rect titleRect => new Rect(rect.x, rect.y, rect.width, 45);
        
        public Rect resizeRect => new Rect(rect.x + rect.width - 32, rect.y + rect.height - 32, 32, 32);

        [NonSerialized]
        private List<NodeBase> _draggedNodes = new List<NodeBase>();

        [NonSerialized] 
        private DateTime _lastClickTime = DateTime.Now;
        private int groupsMinized = 0;

        public GraphBox(string p_comment, Rect p_rect)
        {
            comment = p_comment;
            rect = p_rect;
        }

        public void DrawGUI(IViewOwner p_owner, GraphBase p_graph)
        {
            Rect offsetRect = new Rect(rect.x + p_graph.viewOffset.x, rect.y + p_graph.viewOffset.y, rect.width, rect.height);

            GUI.color = color;
            
            GUI.Box(offsetRect, "", p_owner.GetSkin().GetStyle("GraphBox"));

            Rect titleRect = new Rect(offsetRect.x + 12, offsetRect.y, offsetRect.width, 40);
            if (Event.current.type == EventType.MouseDown && titleRect.Contains(Event.current.mousePosition))
            {
                var currentTime = DateTime.Now;
                if ((currentTime  - _lastClickTime).TotalSeconds < 0.3)
                {
                    editingBoxComment = this;
                }
                _lastClickTime = currentTime;
            }

            GUI.color = new Color(color.r, color.g, color.b, .5f);
            GUI.DrawTexture(new Rect(offsetRect.x + offsetRect.width - 30, offsetRect.y + offsetRect.height - 30, 26, 26), TextureUtils.GetTexture("Icons/Resize_Icon"));

            GUI.color = Color.white;
            GUIStyle style = new GUIStyle();
            style.fontStyle = FontStyle.Bold;
            style.fontSize = 24;
            style.normal.textColor = Color.white;
            style.alignment = TextAnchor.LowerLeft;
            if (editingBoxComment == this)
            {
                comment = GUI.TextField(titleRect, comment, style);
            }
            else
            {
                GUI.Label(titleRect, comment, style);
            }
            
        }

        public void StartDrag(GraphBase p_graph)
        {
            if (moveNodes)
            {
                _draggedNodes = p_graph.Nodes.FindAll(n =>
                    rect.Contains(new Vector2(n.rect.x, n.rect.y)) &&
                    rect.Contains(new Vector2(n.rect.x + n.rect.width, n.rect.y + n.rect.height)));
            }
        }

        public void Drag(Vector2 p_offset)
        {
            if (moveNodes)
                _draggedNodes.ForEach(n => n.rect.position += p_offset);

            rect.position += p_offset;
        }
        
        public void StartResize()
        {
        }

        public void Resize(Vector2 p_offset)
        {
            rect.xMax += p_offset.x;
            rect.yMax += p_offset.y;
        }

        public virtual bool DrawInspector(IViewOwner p_owner)
        {
            bool initializeMinimization = false;
            if (groupsMinized == -1)
            {
                initializeMinimization = true;
                groupsMinized = 0;
            }
            
            GUILayout.Space(2);
            
            GUIStyle minStyle = GUIStyle.none;
            minStyle.normal.textColor = Color.white;
            minStyle.fontSize = 16;
            
            var fields = this.GetType().GetFields();
            Array.Sort(fields, NodeSort.GroupSort);
            string lastGroup = "";
            bool lastGroupMinimized = false;
            bool invalidate = false;
            int groupIndex = 0;
            foreach (var field in fields)
            {
                if (field.IsConstant() || field.IsStatic) continue;
                
                HideAttribute ha = field.GetCustomAttribute<HideAttribute>();
                if (ha != null)
                    continue;

                TitledGroupAttribute ga = field.GetCustomAttribute<TitledGroupAttribute>();
                string currentGroup = ga != null ? ga.Group : "Properties";
                if (currentGroup != lastGroup)
                {
                    int groupMask = (int)Math.Pow(2, groupIndex);
                    groupIndex++;
                    if (initializeMinimization && ga != null && ga.Minimized && (groupsMinized & groupMask) == 0)
                    {
                        groupsMinized += groupMask;
                    }

                    var isMinimized = (groupsMinized & groupMask) != 0;
                    UniGUIUtils.DrawMinimizableTitle("  " + currentGroup, ref isMinimized, 13);

                    if (isMinimized != ((groupsMinized & groupMask) != 0))
                    {
                        groupsMinized = (groupsMinized & groupMask) == 0
                            ? groupsMinized + groupMask
                            : groupsMinized - groupMask;
                    }
                    
                    //GUIProperties.Separator(16, 2, 4, new Color(0.1f, 0.1f, 0.1f));
                    // GUILayout.Label(currentGroup, p_owner.GetSkin().GetStyle("PropertyGroup"),
                    //     GUILayout.Width(120));
                    // Rect lastRect = GUILayoutUtility.GetLastRect();
                    //
                    //
                    // if (GUI.Button(new Rect(lastRect.x + 302, lastRect.y - 25, 20, 20), (groupsMinized & groupMask) != 0 ? "+" : "-",
                    //     minStyle))
                    // {
                    //     groupsMinized = (groupsMinized & groupMask) == 0
                    //         ? groupsMinized + groupMask
                    //         : groupsMinized - groupMask;
                    // }

                    lastGroup = currentGroup;
                    lastGroupMinimized = (groupsMinized & groupMask) != 0;
                }

                if (lastGroupMinimized)
                    continue;

                invalidate = GUIProperties.PropertyField(field, this, null);
            }

            return invalidate;
        }
    }
}