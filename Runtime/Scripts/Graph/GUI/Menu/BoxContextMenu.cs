/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using UnityEngine;
using UniversalGUI;

namespace Nodemon
{
    public class BoxContextMenu
    {
        private Vector2 _lastMousePosition;

        public void Show(GraphBase p_graph, GraphBox p_region)
        {
            _lastMousePosition = Event.current.mousePosition;
            
            UniGUIGenericMenuPopup.Show(Get(p_graph, p_region), "", _lastMousePosition, 160, 100, false, false);
        }

        public UniGUIGenericMenu Get(GraphBase p_graph, GraphBox p_region) 
        {
            UniGUIGenericMenu menu = new UniGUIGenericMenu();

            menu.AddItem(new GUIContent("Delete Box"), false, () => DeleteBox(p_graph, p_region));

            return menu;
        }
        
        void DeleteBox(GraphBase p_graph, GraphBox p_region)
        {
            UniversalUndo.RegisterCompleteObjectUndo(p_graph, "Delete Box");
            p_graph.DeleteBox((GraphBox)p_region);
        }
    }
}