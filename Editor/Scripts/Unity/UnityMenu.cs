/*
 *	Created by:  Peter @sHTiF Stefcek
 */

#if UNITY_EDITOR

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Nodemon.Editor
{
    public class UnityMenu
    {
        [MenuItem("Tools/Nodemon/AOT")]
        public static void ShowAOTWindow()
        {
            AOTWindow.Init();
        }
        
        [MenuItem("Tools/UniversalGUI/Use EditorGUI")]
        public static void Reserialize()
        {
            if (GetAllDefines().Contains("USE_EDITORGUI"))
            {
                UnsetEditorGUISymbols();
            }
            else
            {
                SetEditorGUISymbols();
            }         
        }

        [MenuItem("Tools/UniversalGUI/Use EditorGUI", true)]
        private static bool ToggleActionValidate()
        {
            Menu.SetChecked("Tools/UniversalGUI/Use EditorGUI", GetAllDefines().Contains("USE_EDITORGUI"));
            return true;
        }
        
        static List<string> GetAllDefines()
        {
            string definesString =
                PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            return definesString.Split(';').ToList();
        }
    
        static void SetEditorGUISymbols()
        {
            List<string> allDefines = GetAllDefines();
       
            if (!allDefines.Contains("USE_EDITORGUI"))
                allDefines.Add("USE_EDITORGUI");

            Debug.Log(allDefines);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup,
                string.Join(";", allDefines.ToArray()));
            Debug.Log(GetAllDefines());
        }

        static void UnsetEditorGUISymbols()
        {
            List<string> allDefines = GetAllDefines();
            allDefines.Remove("USE_EDITORGUI");
        
            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup,
                string.Join(";", allDefines.ToArray()));
        }
    }
}

#endif