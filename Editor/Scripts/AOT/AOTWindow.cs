/*
 *	Created by:  Peter @sHTiF Stefcek
 */

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Nodemon.Editor
{
    public class AOTWindow : EditorWindow
    {
        private Vector2 _scrollPositionScanned;
        private Vector2 _scrollPositionExplicit;

        private bool _generateLinkXml = true;
        private bool _includeOdin = false;

        private AOTConfig _aotConfig;
        
        [MenuItem("Tools/AOT")]
        public static void Init()
        {
            var instance = GetWindow<AOTWindow>();
            instance.titleContent = new GUIContent("AOT Editor");
            instance.minSize = new Vector2(800, 400);
        }

        private void OnGUI()
        {
            if (_aotConfig == null)
                _aotConfig = AOTConfig.Create();
            

            var rect = new Rect(0, 0, position.width, position.height);
            
            RuntimeGUIUtils.DrawTitle("AOT Scanner/Generator", 13);
            
            var titleStyle = new GUIStyle();
            titleStyle.alignment = TextAnchor.MiddleLeft;
            titleStyle.padding.left = 5;
            titleStyle.normal.textColor = new Color(1, .5f, 0);
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.fontSize = 16;
            
            var infoStyle = new GUIStyle();
            infoStyle.normal.textColor = Color.gray;
            infoStyle.alignment = TextAnchor.MiddleLeft;
            infoStyle.padding.left = 5;
            
            var scrollViewStyle = new GUIStyle();
            scrollViewStyle.normal.background = TextureUtils.GetColorTexture(new Color(.1f, .1f, .1f));
            
            EditorGUI.BeginChangeCheck();

            _aotConfig.assemblyPath = EditorGUILayout.TextField("Assembly Path", _aotConfig.assemblyPath);
            _aotConfig.assemblyName = EditorGUILayout.TextField("Assembly Name", _aotConfig.assemblyName);

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(_aotConfig);
            }
            
            GUILayout.Space(4);
            GUILayout.Label("Explicit Types", titleStyle, GUILayout.ExpandWidth(true));
            GUILayout.Label(
                "You have " +
                (_aotConfig.types == null
                    ? 0
                    : _aotConfig.types.Count) + " explicitly defined types.", infoStyle,
                GUILayout.ExpandWidth(true));
            GUILayout.Space(2);
            
            _scrollPositionExplicit = GUILayout.BeginScrollView(_scrollPositionExplicit, scrollViewStyle,
                GUILayout.ExpandWidth(true), GUILayout.Height(rect.height - 204));
            GUILayout.BeginVertical();
            
            if (_aotConfig.types != null)
            {
                int index = 0;
                foreach (Type type in _aotConfig.types)
                {
                    if (type == null)
                        continue;
                    
                    GUILayout.BeginHorizontal();
                    GUILayout.Label((string.IsNullOrEmpty(type.Namespace) ? "" : type.Namespace + ".") +
                                    type.GetReadableTypeName());
            
                    if (type.IsGenericType && type.GetGenericArguments()[0].FullName == null) 
                    {
                        if (GUILayout.Button("Inflate", GUILayout.Width(120)))
                        {
                            TypeContextMenu.ShowAsPopup((p) => InflateType(p, type, index));
                        }
                    }
                    
                    if (GUILayout.Button("Remove", GUILayout.Width(120)))
                    {
                        _aotConfig.types.Remove(type);
                        EditorUtility.SetDirty(_aotConfig);
                        GUILayout.EndHorizontal();
                        break;
                    }
                    GUILayout.EndHorizontal();
                    index++;
                }
            }
            
            GUILayout.EndVertical();
            GUILayout.EndScrollView();
            
            GUILayout.Space(4);
            GUILayout.BeginVertical();
            
            GUILayout.BeginHorizontal();
            _generateLinkXml =
                GUILayout.Toggle(_generateLinkXml, new GUIContent("Generate Link Xml"), GUILayout.Width(140));
            _includeOdin = GUILayout.Toggle(_includeOdin, new GUIContent("Include Odin Assembly"));
            
            GUILayout.EndHorizontal();
            GUILayout.Space(2);
            GUILayout.BeginHorizontal();
            
            bool generate = GUILayout.Button("Generate AOT DLL", GUILayout.Height(40));
            
            if (GUILayout.Button("Add Explicit Type", GUILayout.Height(40)))
            {
                TypeContextMenu.ShowAsPopup(AddType);
            }
            
            GUILayout.EndHorizontal();
            
            var dll = PluginImporter.GetAtPath(_aotConfig.assemblyPath + "/" +
                                                  _aotConfig.assemblyName+".dll");
            
            if (dll != null)
            {
                GUILayout.Label("Assembly generated in " + _aotConfig.assemblyPath + "/" +
                                _aotConfig.assemblyName + ".dll" + " last generated on " +
                                _aotConfig.assemblyGeneratedTime);
            }
            else
            {
                GUILayout.Label("No generated Dash AOT Assembly found.");
            }

            GUILayout.EndVertical();
            
            if (generate)
            {
                _aotConfig.assemblyGeneratedTime = DateTime.Now;
                EditorUtility.SetDirty(_aotConfig);
                AOTGenerator.GenerateDLL(_aotConfig.types, _aotConfig.assemblyPath, _aotConfig.assemblyName, _generateLinkXml, _includeOdin);
            }
        }

        void InflateType(object p_type, Type p_genericType, int p_index)
        {
            Type[] types = { (Type)p_type };
            Type inflated = p_genericType.MakeGenericType(types);
            _aotConfig.types[p_index] = inflated;
            EditorUtility.SetDirty(_aotConfig);
        }
        
        void AddType(object p_type)
        {
            if (_aotConfig.types == null)
            {
                _aotConfig.types = new List<Type>();
            }

            if (!_aotConfig.types.Contains((Type)p_type))
            {
                _aotConfig.types.Add((Type)p_type);
            }
            EditorUtility.SetDirty(_aotConfig);
        }
    }
}

#endif