/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniversalGUI;

namespace Nodemon
{
    public class SelectionManager
    {
        static public List<NodeBase> copiedNodes = new List<NodeBase>();
        static public List<int> selectedNodes = new List<int>();
        static public List<int> selectingNodes = new List<int>();
        
        static public int connectingIndex = -1;
        static public NodeBase connectingNode = null;
        static public ConnectorType connectingType = ConnectorType.INPUT;
        static public Vector2 connectingPosition;

        static public int SelectedCount => selectedNodes == null ? 0 : selectedNodes.Count;
        
        public static void StartConnectionDrag(NodeBase p_node, int p_connectorIndex, ConnectorType p_connectorType, Vector2 p_mousePosition)
        {
            connectingNode = p_node;
            connectingIndex = p_connectorIndex;
            connectingType = p_connectorType;
            connectingPosition = p_mousePosition;
        }
        
        public static void EndConnectionDrag(GraphBase p_graph, NodeBase p_node = null, int p_index = -1)
        {
            if (p_graph != null && p_node != null && p_index >= 0)
            {
                if (p_node != connectingNode)
                {
                    UniversalUndo.RegisterCompleteObjectUndo(p_graph, "Connect node");

                    switch (connectingType)
                    {
                        case ConnectorType.INPUT:
                            p_graph.Connect(connectingNode, SelectionManager.connectingIndex, p_node, p_index);
                            break;
                        case ConnectorType.OUTPUT:
                            p_graph.Connect(p_node, p_index, connectingNode, connectingIndex);
                            break;
                    }

                    p_graph.MarkDirty();
                }
            }

            connectingNode = null;
            connectingIndex = -1;
        }
        
        public static NodeBase GetSelectedNode(GraphBase p_graph)
        {
            if (p_graph == null)
                return null;

            return (selectedNodes != null && selectedNodes.Count == 1)
                ? p_graph.Nodes[selectedNodes[0]]
                : null;
        }

        public static bool IsSelected(int p_nodeIndex) => selectedNodes.Contains(p_nodeIndex);

        public static bool IsSelected(GraphBase p_graph, NodeBase p_node)
        {
            return p_graph == null ? false : IsSelected(p_graph.Nodes.IndexOf(p_node)); 
        }

        public static bool IsSelecting(int p_nodeIndex) => selectingNodes.Contains(p_nodeIndex);
        
        public static bool IsSelecting(GraphBase p_graph, NodeBase p_node)
        {
            return p_graph == null ? false : IsSelecting(p_graph.Nodes.IndexOf(p_node)); 
        }

        public static void ClearSelection(GraphBase p_graph)
        {
            UniGUI.FocusControl("");
            
            if (p_graph != null)
            {
                selectedNodes.ForEach(n => p_graph.Nodes[n].OnUnselect());
            }
            
            selectedNodes.Clear();
        }
        
        public static void ReindexSelected(int p_index)
        {
            for (int i = 0; i < selectedNodes.Count; i++)
            {
                if (selectedNodes[i] > p_index)
                    selectedNodes[i]--;
            }
        }

        public static void DragSelectedNodes(GraphBase p_graph, Vector2 p_delta)
        {
            selectedNodes.ForEach(n => p_graph.Nodes[n].rect.position += p_delta * p_graph.zoom);
        }
        
        public static void CopyNode(GraphBase p_graph, NodeBase p_node)
        {
            if (p_graph == null)
                return;

            copiedNodes.Clear();
            copiedNodes.Add(p_node);
        }

        public static bool HasCopiedNodes()
        {
            return copiedNodes.Count != 0;
        }
        
        public static void PasteNodes(GraphBase p_graph, Vector3 p_mousePosition, float p_zoom)
        {
            if (p_graph == null || copiedNodes.Count == 0)
                return;
            
            List<NodeBase> newNodes = NodeUtils.DuplicateNodes(p_graph, copiedNodes);
            
            newNodes[0].rect = new Rect(p_mousePosition.x * p_zoom - p_graph.viewOffset.x,
                p_mousePosition.y * p_zoom - p_graph.viewOffset.y, 0, 0);

            for (int i = 1; i < newNodes.Count; i++)
            {
                NodeBase node = newNodes[i];
                node.rect.x = newNodes[0].rect.x + (node.rect.x - copiedNodes[0].rect.x);
                node.rect.y = newNodes[0].rect.y + (node.rect.y - copiedNodes[0].rect.y);
            }
            
            selectedNodes = newNodes.Select(n => n.Index).ToList();

            p_graph.MarkDirty();
        }

        public static void DuplicateSelectedNodes(GraphBase p_graph)
        {
            if (p_graph == null || selectedNodes.Count == 0)
                return;
            
            UniversalUndo.RegisterCompleteObjectUndo(p_graph, "Duplicate Nodes");

            List<NodeBase> nodes = selectedNodes.Select(i => p_graph.Nodes[i]).ToList();
            List<NodeBase> newNodes = NodeUtils.DuplicateNodes(p_graph, nodes);
            selectedNodes = newNodes.Select(n => n.Index).ToList();
            
            p_graph.MarkDirty();
        }
        
        public static void DuplicateNode(GraphBase p_graph, NodeBase p_node)
        {
            if (p_graph == null)
                return;
            
            UniversalUndo.RegisterCompleteObjectUndo(p_graph, "Duplicate Node");

            NodeBase node = NodeUtils.DuplicateNode(p_graph, p_node);
            selectedNodes = new List<int> { node.Index };
            
            p_graph.MarkDirty();
        }
        
        public static void CopySelectedNodes(GraphBase p_graph)
        {
            if (p_graph == null || selectedNodes.Count == 0)
                return;

            copiedNodes = selectedNodes.Select(i => p_graph.Nodes[i]).ToList();
        }
        
        public static void CreateBoxAroundSelectedNodes(GraphBase p_graph)
        {
            List<NodeBase> nodes = selectedNodes.Select(i => p_graph.Nodes[i]).ToList();
            Rect region = nodes[0].rect;
            
            nodes.ForEach(n =>
            {
                if (n.rect.xMin < region.xMin) region.xMin = n.rect.xMin;
                if (n.rect.yMin < region.yMin) region.yMin = n.rect.yMin;
                if (n.rect.xMax > region.xMax) region.xMax = n.rect.xMax;
                if (n.rect.yMax > region.yMax) region.yMax = n.rect.yMax;
            });

            p_graph.CreateBox(region);
            
            p_graph.MarkDirty();
        }
        
        public static void DeleteSelectedNodes(GraphBase p_graph)
        {
            if (p_graph == null || selectedNodes.Count == 0)
                return;
            
            UniversalUndo.RegisterCompleteObjectUndo(p_graph, "Delete Nodes");

            var nodes = selectedNodes.Select(i => p_graph.Nodes[i]).ToList();
            nodes.ForEach(n => p_graph.DeleteNode(n));

            selectedNodes = new List<int>();
            
            p_graph.MarkDirty();
        }
        
        public static void DeleteNode(GraphBase p_graph, NodeBase p_node)
        {
            if (p_graph == null)
                return;
            
            UniversalUndo.RegisterCompleteObjectUndo(p_graph, "Delete Node");

            int index = p_node.Index;
            p_graph.DeleteNode(p_node);
            selectedNodes.Remove(index);
            ReindexSelected(index);
            
            p_graph.MarkDirty();
        }

        public static void AddNodeToSelection(GraphBase p_graph, int p_nodeIndex)
        {
            //p_graph.Nodes[p_nodeIndex].IsDirty = true;
            selectedNodes.Add(p_nodeIndex);
        }

        public static void SelectNode(GraphBase p_graph, NodeBase p_node, bool p_forceView = false)
        {
            UniGUI.FocusControl("");
            selectedNodes.Clear();

            if (p_node == null || p_graph == null)
                return;

            p_node.OnSelect();
            selectedNodes.Add(p_node.Index);

            //p_node.MarkDirty();

            if (p_forceView)
            {
                //p_graph.viewOffset = -p_node.rect.center + zoom * MachinaEditorCore.EditorConfig.editorPosition.size / 2;
            }
        }

        public static void SelectingNodes(List<int> p_nodes)
        {
            selectingNodes = p_nodes;
        }

        public static void SelectingToSelected()
        {
            selectedNodes.Clear();
            selectedNodes.AddRange(selectingNodes);
            selectingNodes.Clear();
        }
        
        public static NodeBase SearchAndSelectNode(GraphBase p_graph, string p_search, int p_index)
        {
            if (p_graph == null)
                return null;

            var searchNodes = p_graph.Nodes.FindAll(n => n.Id.ToLower().Contains(p_search)).ToList();
            if (searchNodes.Count == 0)
                return null;
            
            if (p_index >= searchNodes.Count) p_index = p_index%searchNodes.Count;

            var node = searchNodes[p_index];
            SelectNode(p_graph, node);
            return node;
        }
        
        public static void ArrangeNodes(GraphBase p_graph, NodeBase p_node)
        {
            if (p_graph == null)
                return;
            
            UniversalUndo.RegisterCompleteObjectUndo(p_graph, "Arrange Nodes");

            NodeUtils.ArrangeNodes(p_graph, p_node);
        
            p_graph.MarkDirty();
        }
        
        public static void SelectConnectedNodes(GraphBase p_graph, NodeBase p_node)
        {
            if (p_graph == null)
                return;

            SelectNode(p_graph, p_node);

            SelectOutputs(p_graph, p_node);
        }

        public static void SelectOutputs(GraphBase p_graph, NodeBase p_node)
        {
            var connections = p_graph.Connections.FindAll(c => c.outputNode == p_node);
            connections.ForEach(c =>
            {
                if (!IsSelected(p_graph, c.inputNode))
                {
                    AddNodeToSelection(p_graph, c.inputNode.Index);
                    SelectOutputs(p_graph, c.inputNode);
                }
            });
        }
    }
}