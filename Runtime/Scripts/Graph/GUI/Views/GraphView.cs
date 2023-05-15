/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System.Linq;
using UnityEngine;
using UniversalGUI;

namespace Nodemon
{
    public abstract class GraphView : ViewBase
    {
        private bool _initialized = false;

        // Zooming
        private Rect _zoomedRect;

        // Selection
        private DraggingType _dragging = DraggingType.NONE;
        private Rect _selectedRegion = Rect.zero;

        private bool _rightDrag = false;
        private Vector2 _rightDragStart;

        // Textures
        private Texture _backgroundTexture;

        private GraphViewMenu _graphViewMenu;

        public override void DrawGUI(Event p_event, Rect p_rect)
        {
            if (!_initialized)
            {
                _backgroundTexture = Resources.Load<Texture>("Textures/graph_background");
                GUIScaleUtils.CheckInit();
                _graphViewMenu = new GraphViewMenu(this);
                _initialized = true;
            }
            
            _zoomedRect = new Rect(0, 0, p_rect.width, p_rect.height);
            GUI.color = new Color(0f, .1f, .2f, 1);
            GUI.Box(p_rect, "");

            if (Graph != null)
            {
                // Draw background texture
                GUI.color = new Color(0, 0, 0, .4f);
                GUI.DrawTextureWithTexCoords(_zoomedRect, _backgroundTexture,
                    new Rect(-Graph.viewOffset.x / _backgroundTexture.width,
                        Graph.viewOffset.y / _backgroundTexture.height,
                        Graph.zoom * p_rect.width / _backgroundTexture.width,
                        Graph.zoom * p_rect.height / _backgroundTexture.height), true);
                GUI.color = Color.white;
                
                // Draw graph
                GUIScaleUtils.BeginScale(ref _zoomedRect, new Vector2(p_rect.width/2, p_rect.height/2), Graph.zoom, false, false);
                Graph.DrawGUI(Owner, p_event, _zoomedRect);
                GUIScaleUtils.EndScale();
                
                Graph.DrawComments(Owner, p_rect, false);
                
                DrawHelp(p_rect);
                
                DrawControllerInfo(p_rect);
                
                DrawSelectingRegion(p_rect);
            }

            DrawCustomGUI(Owner, p_event, p_rect);

            DrawTitle(p_rect);
        }

        protected abstract void DrawCustomGUI(IViewOwner p_owner, Event p_event, Rect p_rect);

        void DrawHelp(Rect p_rect)
        {
            if (Graph == null || Graph.Nodes.Count > 0)
                return;

            string helpString = "RIGHT CLICK to create nodes.\n" +
                                "Hold RIGHT mouse button to DRAG around.";
            
            GUIStyle style = new GUIStyle();
            style.fontSize = 18;
            style.normal.textColor = Color.gray;
            style.fontStyle = FontStyle.Bold;
            style.alignment = TextAnchor.UpperCenter;
            GUI.Label(new Rect(p_rect.x, p_rect.y + 30, p_rect.width, p_rect.height), helpString, style);
        }
        
        void DrawSelectingRegion(Rect p_rect)
        {
            if (_dragging == DraggingType.SELECTION)
            {
                GUI.color = new Color(1, 1, 1, 0.1f);
                GUI.DrawTextureWithTexCoords(_selectedRegion, TextureUtils.GetColorTexture(Color.white), new Rect(0,0,64,64), true);
                GUI.color = Color.white;
            }
        }
        
        void DrawControllerInfo(Rect p_rect)
        {
            if (Graph == null)
                return;
            
            GUI.color = Color.white;
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.white;
            style.fontSize = 24;
            style.fontStyle = FontStyle.Bold;
            GUI.color = new Color(1, 1, 1, 0.25f);

            if (Controller != null && (Controller is Object && (Controller as Object) != null))
            {
                style.normal.textColor = Color.yellow;
                style.fontSize = 18;
                GUI.Label(new Rect(p_rect.x + 16, p_rect.height - 78, 200, 40), Controller.name, style);
            }
            
            if (GraphUtils.IsSubGraph(Owner.GetConfig().editingGraphPath))
            {
                if (GUI.Button(new Rect(p_rect.x + 16, p_rect.height - (Controller == null ? 80 : 118), 100, 32), "GO TO PARENT"))
                {
                    if (Controller != null)
                    {
                        Owner.EditController(Controller,
                            GraphUtils.GetParentPath(Owner.GetConfig().editingGraphPath));
                    }
                    else
                    {
                        Owner.EditGraph(Owner.GetConfig().editingRootGraph,
                            GraphUtils.GetParentPath(Owner.GetConfig().editingGraphPath));
                    }
                }
            }
        }

        void DrawTitle(Rect p_rect)
        {
            // Draw title background
            Rect titleRect = new Rect(0, 0, p_rect.width, 24);
            GUI.color = new Color(0.1f, 0.1f, .1f, .8f);
            GUI.DrawTexture(titleRect, TextureUtils.GetColorTexture(Color.white));
            GUI.color = Color.white;

            // Draw graph name
            GUIStyle style = new GUIStyle(UniGUI.Skin.label);
            style.alignment = TextAnchor.MiddleCenter;
            if (Graph != null)
            {
                style.normal.textColor = Color.gray;
                GUI.Label(new Rect(0, 0, p_rect.width, 24), new GUIContent("Editing graph:"), style);
                style.normal.textColor = Color.white;
                style.fontStyle = FontStyle.Bold;
                style.alignment = TextAnchor.MiddleLeft;

                GUI.Label(new Rect(p_rect.width / 2 + 40, 0, p_rect.width, 24),
                    new GUIContent(Owner.GetConfig().editingRootGraph.name + 
                                   (GraphUtils.IsSubGraph(Owner.GetConfig().editingGraphPath)
                                       ? "/" + Owner.GetConfig().editingGraphPath
                                       : "")), style);
            }
            else
            {
                style.normal.textColor = Color.gray;
                GUI.Label(new Rect(0, 0, p_rect.width, 24), new GUIContent("No graph loaded."), style);
            }

            if (Application.isPlaying && Graph != null && (Object)Owner.GetConfig().editingController != null)
            {
                style = new GUIStyle();
                style.fontSize = 18;
                style.normal.textColor = Color.yellow;
                style.alignment = TextAnchor.MiddleCenter;
                GUI.Label(new Rect(0, 32, p_rect.width, 24),
                    new GUIContent("Debugging bound: " + Owner.GetConfig().editingController.name), style);
                GUI.color = Color.white;
            }
            
            // Draw version info
            style = new GUIStyle();
            style.normal.textColor = Color.gray;
            style.alignment = TextAnchor.MiddleRight;
            GUI.Label(new Rect(0 + p_rect.width - 75, 0, 70, 24), Owner.GetVersionLabel(), style);

            _graphViewMenu.Draw(Graph);
        }

        public override void ProcessMouse(Event p_event, Rect p_rect)
        {
            if (!p_event.isMouse && !p_event.isScrollWheel)
                return;
            
            if (Graph == null || !p_rect.Contains(p_event.mousePosition))
                return;

            ProcessMouseMove(Owner, p_event, p_rect);
            
            ProcessMouseWheel(p_event, p_rect);
            
            ProcessLeftClick(Owner, p_event, p_rect);

            ProcessRightClick(p_event, p_rect);
            
            ProcessMouseDrag(p_event, p_rect);

            // When connecting node force editor update fps up
            if (SelectionManager.connectingNode != null)
            {
                Owner.SetDirty(true);
            }
        }

        void ProcessMouseMove(IViewOwner p_owner, Event p_event, Rect p_rect)
        {
            var mousePosition = p_event.mousePosition * Graph.zoom - new Vector2(p_rect.x, p_rect.y);
            NodeBase hitNode;
            ConnectorType connectorType;
            int connectorIndex;
            Graph.HitsNode(p_owner, mousePosition, out hitNode, out connectorType, out connectorIndex);

            if (hitNode != null && hitNode.hasErrorsInExecution)
            {
                p_owner.SetTooltip(hitNode.errorInExecutionMessage);
            }
            else
            {
                p_owner.SetTooltip("");
            }
        }

        void ProcessLeftClick(IViewOwner p_owner, Event p_event, Rect p_rect)
        {
            if (p_event.button != 0)
                return;
            
            if (p_event.type == EventType.MouseDown)
            {
                SelectionManager.EndConnectionDrag(Graph);
                GUI.FocusControl("");
            }
            
            if (p_event.type == EventType.MouseDown && !p_event.alt && Graph != null)
            {
                Owner.SetDirty(true);
                
                var mousePosition = p_event.mousePosition * Graph.zoom - new Vector2(p_rect.x, p_rect.y);
                NodeBase hitNode;
                ConnectorType connectorType;
                int connectorIndex;
                Graph.HitsNode(p_owner, mousePosition, out hitNode, out connectorType, out connectorIndex);
                
                if (hitNode != null)
                {

                    if (connectorIndex >= 0)
                    {
                        SelectionManager.StartConnectionDrag(hitNode, connectorIndex, connectorType, mousePosition);
                    }
                    else
                    {
                        int hitNodeIndex = Graph.Nodes.IndexOf(hitNode);

                        if (!SelectionManager.IsSelected(hitNodeIndex) && (!p_event.shift || hitNodeIndex == 0))
                        {
                            SelectionManager.ClearSelection(Graph);
                        }

                        if (hitNodeIndex >= 0)
                        {
                            AddSelectedNode(hitNodeIndex);

                            _dragging = DraggingType.NODE_DRAG;
                        }
                    }
                }
                else
                {
                    GraphBox box = Graph.HitsBoxDrag(mousePosition);

                    if (box != null)
                    {
                        GraphBox.selectedBox = box;
                        GraphBox.selectedBox.StartDrag(Graph);
                        _dragging = DraggingType.BOX_DRAG;
                    }
                    else
                    {
                        box = Graph.HitsBoxResize(p_event.mousePosition * Graph.zoom - new Vector2(p_rect.x, p_rect.y));

                        if (box != null)
                        {
                            GraphBox.selectedBox = box;
                            GraphBox.selectedBox.StartResize();
                            _dragging = DraggingType.BOX_RESIZE;
                        }
                        else
                        {
                            _dragging = DraggingType.SELECTION;
                            GraphBox.selectedBox = null;
                            _selectedRegion = new Rect(p_event.mousePosition.x, p_event.mousePosition.y, 0, 0);
                        }
                    }
                }
            }

            if (p_event.type == EventType.MouseUp)
            {
                if (SelectionManager.connectingNode != null)
                {
                    var mousePosition = p_event.mousePosition * Graph.zoom - new Vector2(p_rect.x, p_rect.y);
                    NodeBase hitNode;
                    ConnectorType connectorType;
                    int connectorIndex;
                    Graph.HitsNode(p_owner, mousePosition, out hitNode, out connectorType, out connectorIndex);

                    if (hitNode != null)
                    {
                        SelectionManager.EndConnectionDrag(Graph, hitNode, connectorIndex);
                    }
                    else
                    {
                        //SelectionManager.EndConnection();
                        Owner.CreateNodeContextMenu.ShowAsPopup(Graph);
                    }
                }
                
                if (_dragging == DraggingType.SELECTION)
                {
                    SelectionManager.SelectingToSelected();
                }
                
                if (_dragging == DraggingType.NODE_DRAG || _dragging == DraggingType.BOX_DRAG || _dragging == DraggingType.BOX_RESIZE)
                {
                    Graph.MarkDirty();
                }

                _dragging = DraggingType.NONE;
                _selectedRegion = Rect.zero;
                Owner.SetDirty(true);
            }
        }
        
        void ProcessMouseDrag(Event p_event, Rect p_rect)
        {
            if (SelectionManager.connectingNode != null && p_event.type == EventType.MouseDrag)
            {
                var mousePosition = p_event.mousePosition * Graph.zoom - new Vector2(p_rect.x, p_rect.y);
                SelectionManager.connectingPosition = mousePosition;
            }
            
            if (p_event.button == 0 && p_event.alt && p_event.type == EventType.MouseDrag && _dragging == DraggingType.NONE)
            {
                if (Graph != null)
                {
                    Graph.viewOffset += p_event.delta * Graph.zoom;
                    
                    Owner.SetDirty(true);
                }
            }
            
            if (p_event.type == EventType.MouseDrag && p_event.button == 0)
            {
                switch (_dragging)
                {
                    case DraggingType.NODE_DRAG:
                        SelectionManager.DragSelectedNodes(Graph, p_event.delta);
                        break;
                    case DraggingType.BOX_DRAG:
                        GraphBox.selectedBox.moveNodes = !p_event.control;
                        GraphBox.selectedBox.Drag(new Vector2(p_event.delta.x * Graph.zoom, p_event.delta.y * Graph.zoom));
                        break;
                    case DraggingType.BOX_RESIZE:
                        GraphBox.selectedBox.Resize(new Vector2(p_event.delta.x * Graph.zoom, p_event.delta.y * Graph.zoom));
                        break;
                    case DraggingType.SELECTION:
                        _selectedRegion.width += p_event.delta.x;
                        _selectedRegion.height += p_event.delta.y;
                        Rect fixedRect = FixRect(_selectedRegion);
                        SelectionManager.SelectingNodes(Graph.Nodes.FindAll(n => n.IsInsideRect(fixedRect, Graph.zoom)).Select(n => n.Index).ToList());
                        break;
                }

                Owner.SetDirty(true);
            }
        }

        void ProcessRightClick(Event p_event, Rect p_rect)
        {
            if (p_event.button != 1 || Graph == null)
                return;

            if (p_event.type == EventType.MouseDown)
            {
                _rightDragStart = p_event.mousePosition;
            }

            if (p_event.type == EventType.MouseDrag)
            {
                if (_rightDrag)
                {
                    Graph.viewOffset += p_event.delta * Graph.zoom;
                } else if ((p_event.mousePosition - _rightDragStart).magnitude > 5)
                {
                    _rightDrag = true;
                    Graph.viewOffset += (p_event.mousePosition - _rightDragStart) * Graph.zoom;
                }
            }
            
            if (p_event.type == EventType.MouseUp)
            {
                if (!_rightDrag)
                {
                    NodeBase hitNode = Graph.HitsNode(p_event.mousePosition * Graph.zoom - new Vector2(p_rect.x, p_rect.y));
                    if (hitNode != null)
                    {
                        Owner.NodeContextMenu.Show(Graph, hitNode);
                    }
                    else
                    {
                        NodeConnection hitConnection = Graph.HitsConnection(
                            p_event.mousePosition * Graph.zoom - new Vector2(p_rect.x, p_rect.y),
                            12);

                        if (hitConnection != null)
                        {
                            Owner.ConnectionContextMenu.Show(Graph, hitConnection);
                        }
                        else
                        {
                            GraphBox hitRegion =
                                Graph.HitsBoxDrag(p_event.mousePosition * Graph.zoom - new Vector2(p_rect.x, p_rect.y));

                            if (hitRegion != null)
                            {
                                Owner.BoxContextMenu.Show(Graph, hitRegion);
                            }
                            else
                            {
                                Owner.CreateNodeContextMenu.Show(Graph);
                            }
                        }
                    }
                }
                else
                {
                    _rightDrag = false;
                }

                p_event.Use();
            }

            Owner.SetDirty(true);
        }
        
        void ProcessMouseWheel(Event p_event, Rect p_rect)
        {
            if (!p_event.isScrollWheel)
                return;
            
            float zoom = Graph.zoom;
            
            float previousZoom = zoom;
            zoom += p_event.delta.y / 12;
            if (zoom < 1) zoom = 1;
            if (zoom > 4) zoom = 4;
            if (previousZoom != zoom && Graph != null)
            {
                Graph.viewOffset.x += (zoom - previousZoom) * p_rect.width / 2;
                Graph.viewOffset.y += (zoom - previousZoom) * p_rect.height / 2;
            }

            Graph.zoom = zoom;
            Owner.SetDirty(true);
        }

        void AddSelectedNode(int p_nodeIndex)
        {
            if (!SelectionManager.IsSelected(p_nodeIndex))
            {
                SelectionManager.AddNodeToSelection(Graph, p_nodeIndex);
                
                // If the controller is not null autoselect it in hierarchy TODO: maybe put this as setting
                // if (MachinaEditorCore.EditorConfig.editingController != null)
                // {
                //     #if UNITY_EDITOR
                //     UnityEditor.Selection.activeGameObject = MachinaEditorCore.EditorConfig.editingController.gameObject;
                //     #endif
                // }
            }
        }
        
        Rect FixRect(Rect p_rect)
        {
            Rect fixedRect = _selectedRegion;
            if (fixedRect.width < 0)
            {
                fixedRect.x += fixedRect.width;
                fixedRect.width = -fixedRect.width;
            }
            if (fixedRect.height < 0)
            {
                fixedRect.y += fixedRect.height;
                fixedRect.height = -fixedRect.height;
            }

            return fixedRect;
        }

    }
}