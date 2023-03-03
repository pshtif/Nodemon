/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System;
using UnityEngine;
using UnityEngine.Scripting;

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
        };
        
        static private Type[] SupportedUnityTypes =
        {
            typeof(Vector4),
            typeof(Vector3),
            typeof(Vector2),
            typeof(Quaternion),
            typeof(Color),
            // typeof(Sprite),
            // typeof(RectTransform),
            // typeof(Transform),
            // typeof(GameObject),
            // typeof(Button),
            // typeof(Canvas),
        };
        
        [Preserve]
        static void AOTInject()
        {
            var aot = new Variables();
            aot.AddVariable<bool>("bool", true);
            aot.AddVariable<int>("int", 0);
            aot.AddVariable<float>("float", 0);
            aot.AddVariable<string>("string", "");
            aot.AddVariable<Vector2>("Vector2", Vector2.zero);
            aot.AddVariable<Vector3>("Vector3", Vector3.zero);
            aot.AddVariable<Vector4>("Vector4", Vector4.zero);
            aot.AddVariable<Quaternion>("Quaternion", Quaternion.identity);
            aot.AddVariable<Color>("Color", Color.white);
        }
        
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