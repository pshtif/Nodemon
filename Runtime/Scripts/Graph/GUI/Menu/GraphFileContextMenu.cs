/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using UnityEngine;
using UniversalGUI;

namespace Nodemon
{
    public class GraphFileContextMenu
    {
        private Vector2 _lastMousePosition;
        
        public void Show(GraphBase p_graph)
        {
            _lastMousePosition = Event.current.mousePosition;
            
            UniGUIGenericMenuPopup.Show(Get(p_graph), "", _lastMousePosition, 160, 100, false, false);
        }
        
        public UniGUIGenericMenu Get(GraphBase p_graph)
        {
            UniGUIGenericMenu menu = new UniGUIGenericMenu();
            
            menu.AddItem(new GUIContent("Import JSON"), false, () => GraphUtils.ImportJSON(p_graph));
            menu.AddItem(new GUIContent("Export JSON"), false, () => GraphUtils.ExportJSON(p_graph));

            return menu;
        }
    }
}