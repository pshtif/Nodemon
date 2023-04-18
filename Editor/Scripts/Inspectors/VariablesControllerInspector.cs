/*
 *	Created by:  Peter @sHTiF Stefcek
 */
#if UNITY_EDITOR

using Nodemon;
using UnityEditor;

namespace Nodemon
{
    [CustomEditor(typeof(VariablesController))]
    public class VariablesControllerInspector : UnityEditor.Editor
    {
        protected VariablesController variablesController => (VariablesController) target;

        public override void OnInspectorGUI()
        {
            EditorGUIUtility.labelWidth = 100;

            bool invalidate = VariableGUI.DrawVariablesInspector("Variables", variablesController.Variables, variablesController, EditorGUIUtility.currentViewWidth-20);

            if (invalidate)
            {
                PrefabUtility.RecordPrefabInstancePropertyModifications(target);
                EditorUtility.SetDirty(target);
            }
        }
    }
}
#endif