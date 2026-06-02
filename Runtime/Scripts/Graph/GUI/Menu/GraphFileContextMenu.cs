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
        
        // Empty by default — hosts override Get to add their own items
        // (see Machina's MachinaGraphFileContextMenu for the format-aware
        // import/export entries).
        public virtual UniGUIGenericMenu Get(GraphBase p_graph)
            => new UniGUIGenericMenu();
    }
}