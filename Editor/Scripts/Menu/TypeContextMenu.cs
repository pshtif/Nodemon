/*
 *	Created by:  Peter @sHTiF Stefcek
 */

#if UNITY_EDITOR
using System;
using UnityEngine;

namespace Nodemon.Editor
{ 
    public class TypeContextMenu
    {
        static private Vector2 _lastMousePosition;
        
        static public void Show(Action<object> p_callback)
        {
            _lastMousePosition = Event.current.mousePosition;
            
            Get(p_callback).ShowAsEditorMenu();
        }

        static public void ShowAsPopup(Action<object> p_callback)
        {
            #if UNITY_EDITOR
            _lastMousePosition = Event.current.mousePosition;
            #else
            _lastMousePosition = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
            #endif

            GenericMenuPopup.Show(Get(p_callback), "Select Type",  _lastMousePosition, 300, 300, true, false);
        }
        
        static public UniversalGUIGenericMenu Get(Action<object> p_callback)
        {
            UniversalGUIGenericMenu menu = new UniversalGUIGenericMenu();

            Type[] loadedTypes = ReflectionUtils.GetAllTypes();
            foreach (Type type in loadedTypes)
            {
                string path =
                    (string.IsNullOrEmpty(type.Namespace) ? "Without Namespace" : type.Namespace.Replace(".", "/")) + "/" +
                    type.Name;
                menu.AddItem(new GUIContent(path, ""), false, p_callback, type);
            }
            
            return menu;
        }
    }
}
#endif