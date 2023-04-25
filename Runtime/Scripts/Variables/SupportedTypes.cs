/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Scripting;
using UniversalGUI;

namespace Nodemon
{
    public class SupportedTypes
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

        public static Type[] GetSupportedTypes()
        {
            var types = new Type[SupportedBasicTypes.Length + SupportedUnityTypes.Length];
            SupportedBasicTypes.CopyTo(types, 0);
            SupportedUnityTypes.CopyTo(types, SupportedBasicTypes.Length);
            return types;
        }
        
        public static string[] GetSupportedTypeNames()
        {
            string[] names = new string[SupportedBasicTypes.Length + SupportedUnityTypes.Length];
            for (int i = 0; i<SupportedBasicTypes.Length; i++)
            {
                names[i] = "Basic/" + Variable.ConvertToTypeName(SupportedBasicTypes[i]);
            }
            
            for (int i = 0; i<SupportedUnityTypes.Length; i++)
            {
                names[SupportedBasicTypes.Length + i] = "Unity/" + Variable.ConvertToTypeName(SupportedUnityTypes[i]);
            }

            return names;
        }
        
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
        
        // public static void ShowMenu(Action<Type> p_callback, Vector2 p_position)
        // {
        //     UniGUIGenericMenu menu = new UniGUIGenericMenu();
        //
        //     foreach (Type type in SupportedBasicTypes)
        //     {
        //         menu.AddItem(new GUIContent("Basic/" + Variable.ConvertToTypeName(type)), false, () => p_callback(type));
        //     }
        //     
        //     foreach (Type type in SupportedUnityTypes)
        //     {
        //         menu.AddItem(new GUIContent("Unity/" + Variable.ConvertToTypeName(type)), false, () => p_callback(type));
        //     }
        //     
        //     UniGUIGenericMenuPopup.Show(menu, "", p_position, 240, 300, false, false, true);
        // }
        
        public static void ShowVariablesMenu(Variables p_variables, Vector2 p_position)
        {
            UniGUIGenericMenu menu = new UniGUIGenericMenu();
        
            foreach (Type type in SupportedBasicTypes)
            {
                menu.AddItem(new GUIContent("Basic/" + Variable.ConvertToTypeName(type)), false, () => p_variables.AddNewVariable(type));
            }
            
            foreach (Type type in SupportedUnityTypes)
            {
                menu.AddItem(new GUIContent("Unity/" + Variable.ConvertToTypeName(type)), false, () => p_variables.AddNewVariable(type));
            }
        
            UniGUIGenericMenuPopup.Show(menu, "", p_position, 240, 300, false, false, true);
        }
    }
}