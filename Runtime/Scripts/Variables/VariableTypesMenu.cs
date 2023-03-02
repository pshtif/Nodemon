/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System;
using UnityEngine;
using UnityEngine.UI;

namespace Nodemon
{
    public class VariableTypesMenu
    {
        static private Type[] SupportedBasicTypes =
        {
            typeof(bool),
            typeof(string),
            typeof(int),
            typeof(float),
            typeof(Vector4),
            typeof(Vector3),
            typeof(Vector2),
            typeof(Quaternion)
        };
        
        static private Type[] SupportedUnityTypes =
        {
            typeof(Sprite),
            typeof(RectTransform),
            typeof(Transform),
            typeof(GameObject),
            typeof(Button),
            typeof(Color),
            typeof(Canvas),
        };
        
        public static void Show(Action<Type> p_callback, Vector2 p_position)
        {
            UniversalGUIGenericMenu menu = new UniversalGUIGenericMenu();

            foreach (Type type in SupportedBasicTypes)
            {
                menu.AddItem(new GUIContent("Basic/" + Variable.ConvertToTypeName(type)), false, () => p_callback(type));
            }
            
            foreach (Type type in SupportedUnityTypes)
            {
                menu.AddItem(new GUIContent("Unity/" + Variable.ConvertToTypeName(type)), false, () => p_callback(type));
            }
            
            GenericMenuPopup.Show(menu, "", p_position, 240, 300, false, false, true);
        }
        
        public static void Show(Variables p_variables, Vector2 p_position)
        {
            UniversalGUIGenericMenu menu = new UniversalGUIGenericMenu();

            foreach (Type type in SupportedBasicTypes)
            {
                menu.AddItem(new GUIContent("Basic/" + Variable.ConvertToTypeName(type)), false, () => p_variables.AddNewVariable(type));
            }
            
            foreach (Type type in SupportedUnityTypes)
            {
                menu.AddItem(new GUIContent("Unity/" + Variable.ConvertToTypeName(type)), false, () => p_variables.AddNewVariable(type));
            }

            GenericMenuPopup.Show(menu, "", p_position, 240, 300, false, false, true);
        }
    }
}