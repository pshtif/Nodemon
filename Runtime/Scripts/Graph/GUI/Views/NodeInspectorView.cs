/*
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
        public int maxHeight = 380;
        
        private float _previousHeight = -1;
        private Vector2 _scrollPosition;

        protected object _previouslyInspected;

        public NodeInspectorView()
        {

        }

        public override Rect? GetOcclusionRect(Rect p_canvasRect)
        {
            if (Graph == null) return null;
            if (SelectionManager.GetSelectedNode(Graph) != null)
            {
                // Use maxHeight + 38 (worst-case panel size) rather than the
                // last-measured _previousHeight. Reason: _previousHeight is
                // only set on Repaint, so on the very first click after
                // selecting a node (or after content grows) the click-block
                // would be smaller than the visible panel; clicks falling
                // through to GraphView.ProcessLeftClick then call
                // GUI.FocusControl("") which clears any text field's keyboard
                // focus and breaks typing into the inspector. Over-occluding
                // the right strip of the editor by ~200px is a far smaller
                // wart than losing keyboard input entirely.
                return new Rect(p_canvasRect.width - 400, 30, 390, maxHeight + 38);
            }
            if (GraphBox.selectedBox != null)
            {
                // GraphBox properties panel is fixed at 340 in DrawBoxGUI.
                return new Rect(p_canvasRect.width - 400, 30, 390, 340);
            }
            return null;
        }

        public override void DrawGUI(Event p_event, Rect p_rect)
        {
            if (Graph == null)
                return;

            var selectedNode = SelectionManager.GetSelectedNode(Graph);
            
            if (selectedNode != null)
            {
                DrawNodeGUI(p_rect);
                if (_previouslyInspected != selectedNode) GUI.FocusControl("");
                _previouslyInspected = selectedNode;
            } else if (GraphBox.selectedBox != null)
            {
                DrawBoxGUI(p_rect);
                if (_previouslyInspected != GraphBox.selectedBox) GUI.FocusControl("");
                _previouslyInspected = GraphBox.selectedBox;
            }
        }

        private void DrawBoxGUI(Rect p_rect)
        {
            Rect rect = new Rect(p_rect.width - 400, 30, 390, 340);
            
            DrawBoxGUI(rect, "Properties", TextAnchor.UpperRight, Color.white);

            GUILayout.BeginArea(new Rect(rect.x+5, rect.y+30, rect.width-10, rect.height-35));

            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, false, false);

            GraphBox.selectedBox.DrawInspector(Owner);

            GUILayout.EndScrollView();
            GUILayout.EndArea();

            UseEvent(rect);
        }

        private void DrawNodeGUI(Rect p_rect) 
        {
            var selectedNode = SelectionManager.GetSelectedNode(Graph);

            Rect rect = new Rect(p_rect.width - 400, 30, 390, _previousHeight + 38);
            
            DrawBoxGUI(rect, "Properties", TextAnchor.UpperRight, Color.white);

            string nodeType = NodeBase.GetNodeNameFromType(selectedNode.GetType());
            GUI.Label(new Rect(rect.x + 5, rect.y, 100, 100), nodeType, Owner.GetSkin().GetStyle("NodePropertiesTitle"));
            
            DrawScriptButton(rect, selectedNode.GetType());

            var inspectorRect = new Rect(rect.x + 5, rect.y + 30, rect.width - 10, rect.height - 35);
            GUILayout.BeginArea(inspectorRect);
            
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, false, false);

            GUIProperties.fieldWidth = 190;
            selectedNode.DrawInspector(Owner);
            GUIProperties.fieldWidth = 0;

            if (Event.current.type == EventType.Repaint)
            {
                var lastRect = GUILayoutUtility.GetLastRect();
                GUILayout.EndScrollView();
                GUILayout.EndArea();

                var lastHeight = Mathf.Min(lastRect.y + lastRect.height, maxHeight);
                if (lastHeight != _previousHeight)
                {
                    _previousHeight = lastHeight;
                    // Hack for faster repaint correction
                    Owner.Repaint();
                }
            }
            else
            {
                GUILayout.EndScrollView();
                GUILayout.EndArea();
            }

            // Match hit-test to the actual drawn rect. A maxHeight-based hit
            // area over-occluded the right side of the editor; the host's
            // GetOcclusionRect-driven type-mute is the primary block path
            // anyway, this UseEvent is just a backstop.
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