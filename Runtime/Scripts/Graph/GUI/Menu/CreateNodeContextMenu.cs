/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System;
using System.Collections.Generic;
using Nodemon.Attributes;
using OdinSerializer.Utilities;
using UnityEngine;
using UniversalGUI;

namespace Nodemon
{ 
    public class CreateNodeContextMenu
    {
        private Vector2 _lastMousePosition;
        
        public void Show(GraphBase p_graph)
        {
            _lastMousePosition = Event.current.mousePosition;
            
            UniGUIGenericMenuPopup.Show(Get(p_graph), "Create Node", _lastMousePosition);
        }

        public void ShowAsPopup(GraphBase p_graph)
        {
            _lastMousePosition = Event.current.mousePosition;

            UniGUIGenericMenuPopup.Show(Get(p_graph), "Create Node", _lastMousePosition);
        }
        
        public UniGUIGenericMenu Get(GraphBase p_graph)
        {
            UniGUIGenericMenu menu = new UniGUIGenericMenu();
            
            if (p_graph != null)
            {
                List<Type> nodeTypes = TypeUtils.GetAllAssignableTypes(typeof(NodeBase));
                foreach (Type type in nodeTypes)
                {
                    // if (IsExperimental(type) && !MachinaEditorCore.EditorConfig.showExperimental)
                    //     continue;

                    if (IsHidden(type))
                        continue;

                    if (CheckMultiple(p_graph, type))
                        continue;
                        
                    CategoryAttribute attribute = type.GetCustomAttribute<CategoryAttribute>();
                    string category = attribute == null ? "Other" : attribute.type;
                    string categoryString = category.ToString();
                    categoryString = categoryString.Substring(0, 1) + categoryString.Substring(1).ToLower();

                    string node;
                    ContextNameAttribute contextNameAttribute = type.GetCustomAttribute<ContextNameAttribute>();
                    if (contextNameAttribute != null)
                    {
                        node = contextNameAttribute.name;
                    }
                    else
                    {
                        node = type.ToString().Substring(type.ToString().IndexOf(".") + 1);
                        node = node.Substring(0, node.Length-4);
                    }
                    
                    menu.AddItem(new GUIContent(categoryString+"/"+node), false, () => CreateNode(p_graph, type));
                }
            }
            
            if (SelectionManager.HasCopiedNodes())
            {
                menu.AddItem(new GUIContent("Paste Nodes"), false, () => PasteNodes(p_graph));
            }

            return menu;
        }

        bool IsHidden(Type p_type)
        {
            CategoryAttribute attribute = p_type.GetCustomAttribute<CategoryAttribute>();

            if (attribute != null && attribute.type == "Hidden")
                return true;

            return false;
        }
        
        bool IsExperimental(Type p_type)
        {
            return p_type.GetCustomAttribute<ExperimentalAttribute>() != null;
        }
        
        bool CheckMultiple(GraphBase p_graph, Type p_type)
        {
            SettingsAttribute sa = (SettingsAttribute) p_type.GetCustomAttribute<SettingsAttribute>();
            if (sa != null && !sa.canHaveMultiple && p_graph.HasNodeOfType(p_type))
                return true;

            return false;
        }

        void CreateNode(GraphBase p_graph, Type p_nodeType)
        {
            Vector2 offset = p_graph.viewOffset;
            Vector2 position = new Vector2(_lastMousePosition.x * p_graph.zoom - offset.x, _lastMousePosition.y * p_graph.zoom - offset.y);
            
            var node = NodeUtils.CreateNode(p_graph, p_nodeType, position);

            if (SelectionManager.connectingNode != null)
            {
                SelectionManager.EndConnectionDrag(p_graph, node, 0);
            }
        }
        
        void PasteNodes(GraphBase p_graph)
        {
            SelectionManager.PasteNodes(p_graph, _lastMousePosition, p_graph.zoom);
        }
    }
}