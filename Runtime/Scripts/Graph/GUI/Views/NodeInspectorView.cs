﻿/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System;
using System.Reflection;
using Nodemon.Attributes;
using UnityEngine;
using UniversalGUI;
using GUI = UnityEngine.GUI;

namespace Nodemon
{
    public class NodeInspectorView : ViewBase
    {
        private Vector2 scrollPosition;

        protected object _previouslyInspected;

        public NodeInspectorView()
        {

        }

        public override void DrawGUI(Event p_event, Rect p_rect)
        {
            if (Graph == null)
                return;

            var selectedNode = SelectionManager.GetSelectedNode(Graph);
            
            if (selectedNode != null)
            {
                DrawGraphNodeGUI(p_rect);
                if (_previouslyInspected != selectedNode) GUI.FocusControl("");
                _previouslyInspected = selectedNode;
            } else if (GraphBox.selectedBox != null)
            {
                DrawGraphBoxGUI(p_rect);
                if (_previouslyInspected != GraphBox.selectedBox) GUI.FocusControl("");
                _previouslyInspected = GraphBox.selectedBox;
            }
        }

        private void DrawGraphBoxGUI(Rect p_rect)
        {
            Rect rect = new Rect(p_rect.width - 400, 30, 390, 340);
            
            DrawBoxGUI(rect, "Properties", TextAnchor.UpperRight, Color.white);

            GUILayout.BeginArea(new Rect(rect.x+5, rect.y+30, rect.width-10, rect.height-35));

            scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, false);

            GraphBox.selectedBox.DrawInspector(Owner);

            GUILayout.EndScrollView();
            GUILayout.EndArea();

            UseEvent(rect);
        }

        private void DrawGraphNodeGUI(Rect p_rect) 
        {
            var selectedNode = SelectionManager.GetSelectedNode(Graph);
            
            InspectorHeightAttribute heightAttibute = selectedNode.GetType().GetCustomAttribute<InspectorHeightAttribute>();
            float height = heightAttibute != null ? heightAttibute.height : 340;

            Rect rect = new Rect(p_rect.width - 400, 30, 390, height);
            
            DrawBoxGUI(rect, "Properties", TextAnchor.UpperRight, Color.white);

            string nodeType = NodeBase.GetNodeNameFromType(selectedNode.GetType());
            GUI.Label(new Rect(rect.x + 5, rect.y, 100, 100), nodeType, Owner.GetSkin().GetStyle("NodePropertiesTitle"));
            
            DrawScriptButton(rect, selectedNode.GetType());

            var inspectorRect = new Rect(rect.x + 5, rect.y + 30, rect.width - 10, rect.height - 35);
            GUILayout.BeginArea(inspectorRect);
            
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, false);

            GUIProperties.fieldWidth = 190;
            selectedNode.DrawInspector(Owner);
            GUIProperties.fieldWidth = 0;
            
            GUILayout.EndScrollView();
            GUILayout.EndArea();

            UseEvent(rect);
        }
        
        void DrawScriptButton(Rect p_rect, Type p_type)
        {
            #if UNITY_EDITOR
            if (GUI.Button(new Rect(p_rect.x + 290, p_rect.y + 7, 16, 16), TextureUtils.GetTexture("Icons/Script_Icon"),
                    GUIStyle.none)) 
            {
                UnityEditor.AssetDatabase.OpenAsset(EditorUtils.GetScriptFromType(p_type), 1);
            }
            #endif
        }
    }
}