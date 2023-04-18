/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using Nodemon;
using UnityEngine;
using UniversalGUI;

namespace Nodemon
{
    public class NodeContextMenu
    {
        private Vector2 _lastMousePosition;
        
        public void Show(GraphBase p_graph, NodeBase p_node)
        {
            _lastMousePosition = Event.current.mousePosition;
            
            UniGUIGenericMenuPopup.Show(Get(p_graph, p_node), "", _lastMousePosition, 160, 100, false, false);
        }
        
        public UniGUIGenericMenu Get(GraphBase p_graph, NodeBase p_node)
        {
            UniGUIGenericMenu menu = new UniGUIGenericMenu();
            
            if (SelectionManager.SelectedCount > 1)
            {
                menu.AddItem(new GUIContent("Copy Nodes"), false, () => CopyNode(p_graph, null));
                menu.AddItem(new GUIContent("Delete Nodes"), false, () => DeleteNode(p_graph, null));
                menu.AddItem(new GUIContent("Duplicate Nodes"), false, () => DuplicateNode(p_graph, null));
                menu.AddItem(new GUIContent("Create Box"), false, () => CreateBox(p_graph));
            }
            else
            {
                menu.AddItem(new GUIContent("Copy Nodes"), false, () => CopyNode(p_graph, p_node));
                menu.AddItem(new GUIContent("Delete Node"), false, () => DeleteNode(p_graph, p_node));   
                menu.AddItem(new GUIContent("Duplicate Node"), false, () => DuplicateNode(p_graph, p_node));
                
                menu.AddSeparator("");
                if (p_node.HasComment())
                {
                    menu.AddItem(new GUIContent("Remove Comment"), false, p_node.RemoveComment);
                }
                else
                {
                    menu.AddItem(new GUIContent("Create Comment"), false, p_node.CreateComment);
                }
            }

            return menu;
        }
        
        void DeleteNode(GraphBase p_graph, NodeBase p_node)
        {
            if (p_node == null)
            {
                SelectionManager.DeleteSelectedNodes(p_graph);
            }
            else
            {
                SelectionManager.DeleteNode(p_graph, p_node);
            }
            
            p_graph.MarkDirty();
        }

        void DuplicateNode(GraphBase p_graph, NodeBase p_node)
        {
            if (p_node == null)
            {
                SelectionManager.DuplicateSelectedNodes(p_graph);
            }
            else
            {
                SelectionManager.DuplicateNode(p_graph, p_node);
            }
        }
        
        void CreateBox(GraphBase p_graph)
        {
            UniversalUndo.RegisterCompleteObjectUndo(p_graph, "Create Box");
            
            SelectionManager.CreateBoxAroundSelectedNodes(p_graph);
        }

        void CopyNode(GraphBase p_graph, NodeBase p_node)
        {
            if (p_node == null)
            {
                SelectionManager.CopySelectedNodes(p_graph);
            }
            else
            {
                SelectionManager.CopyNode(p_graph ,p_node);
            }
        }
    }
}