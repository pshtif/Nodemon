/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System;
using Nodemon;
using UnityEngine;
using UniversalGUI;

namespace Nodemon
{
    public class TypesMenu
    {
        static private Type[] SupportedBasicTypes =
        {
            typeof(bool),
            typeof(int),
            typeof(float),
            typeof(string),
            typeof(Vector3),
            typeof(Vector2),
            typeof(Quaternion)
        };
        
        static private Type[] SupportedUnityTypes =
        {
            typeof(Sprite),
            typeof(GameObject),
            typeof(Mesh),
            typeof(Material),
            typeof(MeshFilter),
            typeof(MeshRenderer)
        };

        private Vector2 _lastMousePosition;
        
        public void Show(Action<Type> p_callback)
        {
            _lastMousePosition = Event.current.mousePosition;
            
            UniGUIGenericMenuPopup.Show(Get(p_callback), "", _lastMousePosition, 160, 100, false, false, true);
        }
        
        public UniGUIGenericMenu Get(Action<Type> p_callback)
        {
            UniGUIGenericMenu menu = new UniGUIGenericMenu();

            foreach (Type type in SupportedBasicTypes)
            {
                menu.AddItem(new GUIContent("Basic/" + Variable.ConvertToTypeName(type)), false, () => p_callback(type));
            }
            
            foreach (Type type in SupportedUnityTypes)
            {
                menu.AddItem(new GUIContent("Unity/" + Variable.ConvertToTypeName(type)), false, () => p_callback(type));
            }

            return menu;
        }
    }
}