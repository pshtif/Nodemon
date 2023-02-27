/*
 *	Created by:  Peter @sHTiF Stefcek
 */

#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

namespace Nodemon
{
    public class EditorSkinUtils
    {
        [MenuItem("Tools/sHTiF/Save Editor Skin")]
        static public void SaveEditorSkin()
        {
            GUISkin skin = ScriptableObject.Instantiate(EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector)) as GUISkin;
            AssetDatabase.CreateAsset(skin, "Assets/EditorSkin.guiskin");
        }

        [MenuItem("Tools/sHTiF/Extract Editor Images")]
        static public void SaveEditorSkinImages()
        {
            GUISkin skin = ScriptableObject.Instantiate(EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector)) as GUISkin;
            var original = skin.button.normal.background;
            Texture2D copyTexture = new Texture2D(original.width, original.height);
            copyTexture.SetPixels(original.GetPixels());
            copyTexture.Apply();
            AssetDatabase.CreateAsset(copyTexture, "Assets/button.asset");
        }
    }
}

#endif