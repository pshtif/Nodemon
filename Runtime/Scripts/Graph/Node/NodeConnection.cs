/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System;
using OdinSerializer;
using UnityEngine;
using LineRenderer = Nodemon.LineRenderer;

namespace Nodemon
{
    [Serializable]
    public class NodeConnection
    {
        public bool active = true;

        public int inputIndex;
        public int outputIndex;
        
        [OdinSerialize]
        public NodeBase inputNode;
        [OdinSerialize]
        public NodeBase outputNode;
        
        #if UNITY_EDITOR
        [NonSerialized] 
        public float executeTime = 0;
        #endif

        public bool IsValid()
        {
            return inputNode != null && outputNode != null;
        }

        public NodeConnection(int p_inputIndex, NodeBase p_inputNode, int p_outputIndex, NodeBase p_outputNode)
        {
            inputIndex = p_inputIndex;
            inputNode = p_inputNode;

            outputIndex = p_outputIndex;
            outputNode = p_outputNode;
        }
        
        public void Draw()
        {
            if (inputNode == null || outputNode == null)
                return;

            Rect outputRect = outputNode.GetConnectorRect(ConnectorType.OUTPUT, outputIndex);
            Vector3 startPos = new Vector3(outputRect.x + outputRect.width / 2, outputRect.y + outputRect.height / 2);

            Rect inputRect = inputNode.GetConnectorRect(ConnectorType.INPUT, inputIndex);
            Vector3 endPos = new Vector3(inputRect.x + inputRect.width / 2, inputRect.y + inputRect.height / 2);
            
            Color connectionColor = active ? new Color(0.0f, 0.6f, .8f) : new Color(0.3f, 0.3f, .3f);
            
            DrawBezier(startPos, endPos, connectionColor, true);
        }
        
        static public void DrawBezier(Vector3 p_startPos, Vector3 p_endPos, Color p_color, bool p_shadow)
        {
            // #if UNITY_EDITOR
            // UnityEditor.Handles.BeginGUI();
            // #endif
            
            Vector3 startTan = p_startPos + Vector3.up * 50;
            Vector3 endTan = p_endPos + Vector3.down * 50;
            Color shadowColor = new Color(0, 0, 0, .06f);

            if (p_shadow)
            {
                for (int i = 0; i < 3; ++i)
                {
                    LineRenderer.DrawBezier(p_startPos, p_endPos, startTan, endTan, shadowColor, null, (i + 1) * 6);
                    //#if UNITY_EDITOR
                    //UnityEditor.Handles.DrawBezier(p_startPos, p_endPos, startTan, endTan, shadowColor, null, (i + 1) * 6);
                    //#endif
                }
            }

            LineRenderer.DrawBezier(p_startPos, p_endPos, startTan, endTan, p_color, null, 4);
            //#if UNITY_EDITOR
            //UnityEditor.Handles.DrawBezier(p_startPos, p_endPos, startTan, endTan, p_color, null, 4);
            //#endif

            //#if UNITY_EDITOR
            //UnityEditor.Handles.EndGUI();
            //#endif
        }

        static public void DrawConnectionToMouse(NodeBase p_node, int p_connectorIndex, ConnectorType p_connectorType, Vector2 p_mousePosition)
        {
            if (p_node == null)
                return;
            
            Rect connectorRect = p_node.GetConnectorRect(p_connectorType, p_connectorIndex);
            Vector3 connectorPos = new Vector3(connectorRect.x + connectorRect.width / 2,
                connectorRect.y + connectorRect.height / 2);

            switch (p_connectorType)
            {
                case ConnectorType.INPUT:
                    DrawBezier(p_mousePosition, connectorPos, Color.green, false);
                    break;
                case ConnectorType.OUTPUT:
                    DrawBezier(connectorPos, p_mousePosition, Color.green, false);
                    break;
            }
        }
        
        public bool Hits(Vector2 p_position, float p_distance)
        {
            Rect outputRect = outputNode.GetConnectorRect(ConnectorType.OUTPUT, outputIndex);
            Vector3 startPos = new Vector3(outputRect.x + outputRect.width / 2, outputRect.y + outputRect.height / 2);

            Rect inputRect = inputNode.GetConnectorRect(ConnectorType.INPUT, inputIndex);
            Vector3 endPos = new Vector3(inputRect.x + inputRect.width / 2, inputRect.y + inputRect.height / 2);
            
            Vector3 startTan = startPos + Vector3.up * 50;
            Vector3 endTan = endPos + Vector3.down * 50;

            #if UNITY_EDITOR
            if (UnityEditor.HandleUtility.DistancePointBezier(new Vector3(p_position.x, p_position.y, 0), startPos, endPos,
                startTan, endTan) < p_distance)
            {
                return true; 
            }
            #endif

            return false;
        }
    }
}