/*
 *	Created by:  Peter @sHTiF Stefcek
 */

#if UNITY_EDITOR

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace Nodemon.Editor
{
    public class UnityMenu
    {
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
        
        static NamedBuildTarget CurrentTarget =>
            NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup);

        static List<string> GetAllDefines()
            => PlayerSettings.GetScriptingDefineSymbols(CurrentTarget).Split(';').ToList();

        static void SetEditorGUISymbols()
        {
            List<string> allDefines = GetAllDefines();
            if (!allDefines.Contains("USE_EDITORGUI"))
                allDefines.Add("USE_EDITORGUI");

            PlayerSettings.SetScriptingDefineSymbols(CurrentTarget, string.Join(";", allDefines));
            Debug.Log(GetAllDefines());
        }

        static void UnsetEditorGUISymbols()
        {
            List<string> allDefines = GetAllDefines();
            allDefines.Remove("USE_EDITORGUI");

            PlayerSettings.SetScriptingDefineSymbols(CurrentTarget, string.Join(";", allDefines));
        }
    }
}

#endif