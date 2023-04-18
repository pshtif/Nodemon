/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System;
using System.Collections.Generic;
using System.Reflection;
using Nodemon.Attributes;
using UnityEngine;

namespace Nodemon
{
    public class NodeUtils
    {
        static public bool CanHaveMultipleInstances(Type p_nodeType)
        {
            SettingsAttribute attribute = (SettingsAttribute) Attribute.GetCustomAttribute(p_nodeType, typeof(SettingsAttribute));
            return attribute == null ? true : attribute.canHaveMultiple;
        }
        
        public static string GetCategoryLabel(Type p_type)
        {
            CategoryAttribute attribute = p_type.GetCustomAttribute<CategoryAttribute>();
            string category = attribute == null ? "Other" : attribute.type;
            string categoryLabel = attribute == null ? string.Empty : attribute.label;
            if (OdinSerializer.Utilities.StringExtensions.IsNullOrWhitespace(categoryLabel))
            {
                categoryLabel = category.ToString();
                categoryLabel = categoryLabel.Substring(0, 1) + categoryLabel.Substring(1).ToLower();
            }

            return categoryLabel;
        }
        
        public static string GetNodeLabel(Type p_nodeType)
        {
            return p_nodeType.ToString().Substring(p_nodeType.ToString().IndexOf(".") + 1);
        }
        
        public static string GetNodeCategory(Type p_type)
        {
            CategoryAttribute attribute = p_type.GetCustomAttribute<CategoryAttribute>();
            string category = attribute == null ? "Other" : attribute.type;

            return category;
        }

        public static NodeBase CreateNode(GraphBase p_graph, Type p_nodeType, Vector2 p_position)
        {
            if (!CanHaveMultipleInstances(p_nodeType) && p_graph.GetNodeByType(p_nodeType) != null)
                return null;
            
            UniversalUndo.RegisterCompleteObjectUndo(p_graph, "Create "+NodeBase.GetNodeNameFromType(p_nodeType));
            
            NodeBase node = NodeBase.Create(p_nodeType, p_graph);

            if (node != null)
            {
                node.rect = new Rect(p_position.x, p_position.y, 0, 0);
                p_graph.Nodes.Add(node);
            }

            p_graph.MarkDirty();

            return node;
        }
        
        public static NodeBase DuplicateNode(GraphBase p_graph, NodeBase p_node)
        {
            NodeBase clone = p_node.Clone(p_graph);
            clone.rect = new Rect(p_node.rect.x + 20, p_node.rect.y + 20, 0, 0);
            p_graph.Nodes.Add(clone);
            return clone;
        }
        
        public static List<NodeBase> DuplicateNodes(GraphBase p_graph, List<NodeBase> p_nodes)
        {
            if (p_nodes == null || p_nodes.Count == 0)
                return null;

            List<NodeBase> newNodes = new List<NodeBase>();
            foreach (NodeBase node in p_nodes)
            {
                NodeBase clone = node.Clone(p_graph);
                clone.rect = new Rect(node.rect.x + 20, node.rect.y + 20, 0, 0);
                p_graph.Nodes.Add(clone);
                newNodes.Add(clone);
            }

            GraphBase originalGraph = p_nodes[0].Graph;
            // Recreate connections within duplicated part
            foreach (NodeBase node in p_nodes)
            {
                List<NodeConnection> connections =
                    originalGraph.Connections.FindAll(c => c.inputNode == node && p_nodes.Contains(c.outputNode));
                foreach (NodeConnection connection in connections)
                {
                    p_graph.Connect(newNodes[p_nodes.IndexOf(connection.inputNode)], connection.inputIndex,
                        newNodes[p_nodes.IndexOf(connection.outputNode)], connection.outputIndex);
                }
            }

            return newNodes;
        }

        public static bool IsNodeOutput(GraphBase p_graph, NodeBase p_outputNode, NodeBase p_node)
        {
            return p_graph.Connections.Exists(c => c.outputNode == p_outputNode && c.inputNode == p_node);
        }

        public static NodeConnection GetConnection(GraphBase p_graph, NodeBase p_inputNode, NodeBase p_outputNode)
        {
            var connection = p_graph.Connections.Find(c => c.outputNode == p_outputNode && c.inputNode == p_inputNode);
            return connection;
        }

        private static List<NodeBase> _arrangedNodes;
        public static void ArrangeNodes(GraphBase p_graph, NodeBase p_node)
        {
            _arrangedNodes = new List<NodeBase>();
            
            ArrangeNodeInternal(p_graph, p_node, null, 0);
        }

        private static void ArrangeNodeInternal(GraphBase p_graph, NodeBase p_node, NodeBase p_byNode, int p_index)
        {
            if (_arrangedNodes.Contains(p_node))
                return;
            
            _arrangedNodes.Add(p_node);
            
            if (p_byNode != null)
            {
                p_node.rect.position = p_byNode.rect.position + new Vector2(p_byNode.rect.width + 40, p_index * 120);
            }

            var connections = p_graph.Connections.FindAll(c => c.outputNode == p_node);
            connections.Sort((c1, c2) =>
            {
                return c1.outputIndex.CompareTo(c2.outputIndex);
            });

            int index = 0;
            foreach (var connection in connections)
            {
                ArrangeNodeInternal(p_graph, connection.inputNode, p_node, index++);   
            }
        }
    }
}