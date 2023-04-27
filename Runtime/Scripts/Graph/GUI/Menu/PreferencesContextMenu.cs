/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using UnityEngine;
using UniversalGUI;

namespace Nodemon
{
    public class PreferencesContextMenu
    {
        private Vector2 _lastMousePosition;
        
        public void Show(GraphBase p_graph, ViewConfig p_config)
        {
            _lastMousePosition = Event.current.mousePosition;
            
            UniGUIGenericMenuPopup.Show(Get(p_graph, p_config), "", _lastMousePosition, 160, 100, false, false, true);
        }
        
        public UniGUIGenericMenu Get(GraphBase p_graph, ViewConfig p_config)
        {
            UniGUIGenericMenu menu = new UniGUIGenericMenu();
            
            menu.AddItem(new GUIContent("Auto invalidate"), p_config.autoInvalidate, () => p_config.autoInvalidate = !p_config.autoInvalidate);
            menu.AddItem(new GUIContent("Show Experimental"), p_config.showExperimental, () => p_config.showExperimental = !p_config.showExperimental);
            menu.AddItem(new GUIContent("Show Variables"), p_graph.showVariables, () => p_graph.showVariables = !p_graph.showVariables);
            menu.AddItem(new GUIContent("Show Node Ids"), p_config.showNodeIds, () => p_config.showNodeIds = !p_config.showNodeIds);
            // menu.AddSeparator("");
            // menu.AddItem(new GUIContent("Validate Serialization"), false, p_graph.ValidateSerialization);
            // menu.AddItem(new GUIContent("Cleanup Null"), false, p_graph.RemoveNullReferences);
            // menu.AddItem(new GUIContent("Cleanup Exposed"), false, p_graph.CleanupExposedReferenceTable);

            return menu;
        }
    }
}