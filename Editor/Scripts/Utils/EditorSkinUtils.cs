/*
 *	Created by:  Peter @sHTiF Stefcek
 */

#if UNITY_EDITOR && MACHINA_DEV // skin-maintenance tooling for Nodemon itself — developer-only

using UnityEngine;
using UnityEditor;

namespace Nodemon
{
    public class EditorSkinUtils
    {
        [MenuItem("Tools/Machina/Developer/Save Editor Skin", false, 130)]
        static public void SaveEditorSkin()
        {
            GUISkin skin = ScriptableObject.Instantiate(EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector)) as GUISkin;
            AssetDatabase.CreateAsset(skin, "Assets/EditorSkin.guiskin");
        }

        [MenuItem("Tools/Machina/Developer/Extract Editor Images", false, 131)]
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