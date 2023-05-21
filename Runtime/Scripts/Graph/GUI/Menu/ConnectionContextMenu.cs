/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using UnityEngine;
using UniversalGUI;

namespace Nodemon
{
    public class ConnectionContextMenu
    {
        private Vector2 _lastMousePosition;
        
        public void Show(GraphBase p_graph, NodeConnection p_connection)
        {
            _lastMousePosition = Event.current.mousePosition;
            
            UniGUIGenericMenuPopup.Show(Get(p_graph, p_connection), "", _lastMousePosition, 160, 100, false, false);
        }
        
        public UniGUIGenericMenu Get(GraphBase p_graph, NodeConnection p_connection)
        {
            UniGUIGenericMenu menu = new UniGUIGenericMenu();

            if (p_connection.active)
            {
                menu.AddItem(new GUIContent("Deactivate connection."), false, () => DeactivateConnection(p_graph, p_connection));

            }
            else
            {
                menu.AddItem(new GUIContent("Activate connection."), false, () => ActivateConnection(p_graph, p_connection));
            }

            menu.AddItem(new GUIContent("Delete Connection"), false, () => DeleteConnection(p_graph, p_connection));

            return menu;
        }
        
        void DeleteConnection(GraphBase p_graph, NodeConnection p_connection)
        {
            UniversalUndo.RegisterCompleteObjectUndo(p_graph, "Delete connection.");
            p_graph.Disconnect((NodeConnection)p_connection);
            p_graph.MarkDirty();
        }
        
        void DeactivateConnection(GraphBase p_graph, NodeConnection p_connection)
        {
            var connection = ((NodeConnection) p_connection);
            connection.active = false;
            p_connection.inputNode.MarkDirty();
            p_graph.MarkDirty();
        }
        
        void ActivateConnection(GraphBase p_graph, NodeConnection p_connection)
        {
            var connection = ((NodeConnection) p_connection);
            connection.active = true;
            p_connection.inputNode.MarkDirty();
            p_graph.MarkDirty();
        }
    }
}