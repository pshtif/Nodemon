using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OdinSerializer;
using OdinSerializer.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Nodemon
{
    public abstract class GraphBase : ScriptableObject, ISerializationCallbackReceiver
    {
        public bool variablesViewMinimized = false;
        public bool showVariables = false;
        
        public Variables variables = new Variables();

        protected IGraphController _controller;
        
        public IGraphController Controller { get; protected set; }
        
        [SerializeField]
        protected List<GraphBox> _boxes = new List<GraphBox>();

        [NonSerialized]
        protected GraphBase _parentGraph;

        public GraphBase GetParentGraph()
        {
            return _parentGraph;
        }
        
        public T GetParentGraph<T>() where T : GraphBase
        {
            return (T)_parentGraph;
        }

        public void SetParentGraph(GraphBase p_graph)
        {
            _parentGraph = p_graph;
        }
        
        public string GraphPath
        {
            get
            {
                if (_parentGraph != null)
                    return _parentGraph.GraphPath + "/"+ name;

                return name;
            }
        }
        
        [SerializeField]
        protected List<NodeBase> _nodes = new List<NodeBase>();
        
        public List<NodeBase> Nodes => _nodes;
        
        [SerializeField]
        protected List<NodeConnection> _connections = new List<NodeConnection>();
        
        public List<NodeConnection> Connections => _connections;

        public bool HasNodeOfType(Type p_nodeType)
        {
            return Nodes.Exists(n => p_nodeType.IsAssignableFrom(n.GetType()));
        }
        
        public bool HasNodeOfType<T>() where T : NodeBase
        {
            return Nodes.Exists(n => n is T);
        }
        
        public NodeBase GetNodeByType(Type p_nodeType)
        {
            return Nodes.Find(n => p_nodeType.IsAssignableFrom(n.GetType()));
        }
        
        public T GetNodeByType<T>() where T:NodeBase
        {
            return (T)Nodes.Find(n => typeof(T).IsAssignableFrom(n.GetType()));
        }

        public List<T> GetNodesByType<T>() where T : NodeBase
        {
            return Nodes.FindAll(n => n is T).ConvertAll(n => (T)n);
        }
        
        public NodeBase GetNodeById(string p_id)
        {
            return Nodes.Find(n => n.Id == p_id);
        }
        
        public void Connect(NodeBase p_inputNode, int p_inputIndex, NodeBase p_outputNode, int p_outputIndex)
        {
            bool exists = Connections.Exists(c =>
                c.inputNode == p_inputNode && c.inputIndex == p_inputIndex && c.outputNode == p_outputNode &&
                c.outputIndex == p_outputIndex);

            if (exists)
                return;

            NodeConnection existingConnection = Connections.Find(c => c.inputNode == p_inputNode && c.inputIndex == p_inputIndex);
            
            if (existingConnection != null && !existingConnection.inputNode.GetInputAllowMultiple(existingConnection.inputIndex))
                Disconnect(existingConnection);

            NodeConnection connection = new NodeConnection(p_inputIndex, p_inputNode, p_outputIndex, p_outputNode);

            _connections.Add(connection);

            p_inputNode.MarkDirty();
        }
        
        public void Disconnect(NodeConnection p_connection)
        {
            p_connection.inputNode.MarkDirty();

            _connections.Remove(p_connection);
        }

        public abstract void ExecuteInputs(NodeBase p_node, bool p_initSeed);
        
        public abstract Task ExecuteInputsAsync(NodeBase p_node, bool p_initSeed);

        public bool HasOutputConnected(NodeBase p_node, int p_index)
        {
            return _connections.Exists(c => c.outputNode == p_node && c.outputIndex == p_index);
        }
        
        public bool HasInputConnected(NodeBase p_node, int p_index)
        {
            return _connections.Exists(c => c.inputNode == p_node && c.inputIndex == p_index);
        }
        
        public List<NodeConnection> GetInputConnections(NodeBase p_node)
        {
            return _connections.FindAll(c => c.inputNode == p_node);
        }
        
        public List<NodeConnection> GetOutputConnections(NodeBase p_node)
        {
            return _connections.FindAll(c => c.outputNode == p_node);
        }

        protected void RemoveNodeConnections(NodeBase p_node)
        {
            Connections.RemoveAll(c => c.outputNode == p_node || c.inputNode == p_node);
        }

        public void DeleteNode(NodeBase p_node)
        {
            p_node.MarkDirty();
            _connections.RemoveAll(c => c.inputNode == p_node || c.outputNode == p_node);
            Nodes.Remove(p_node);
            p_node.Remove();
        }
        
        public virtual void MarkDirty()
        {
            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
            #endif

            if (_parentGraph != null)
            {
                _parentGraph.MarkDirty();
            }
        }

#region INTERNALS
        public string GenerateNodeId(NodeBase p_node, string p_id)
        {
            if (string.IsNullOrEmpty(p_id))
            {
                string type = p_node.GetType().ToString();
                int dotIndex = type.IndexOf("."); 
                p_id = type.Substring(dotIndex + 1, type.Length-(dotIndex+5)) + "1";
            }

            while (Nodes.Exists(n => n.Id == p_id))
            {
                string number = string.Concat(p_id.Reverse().TakeWhile(char.IsNumber).Reverse());
                p_id = p_id.Substring(0,p_id.Length-number.Length) + (Int32.Parse(number)+1);
            }

            return p_id;
        }
#endregion
        
#region SERIALIZATION

        [SerializeField, HideInInspector]
        private SerializationData _serializationData;
        
        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            //Debug.Log("OnAfterDeserialize");
            using (var cachedContext = Cache<DeserializationContext>.Claim())
            {
                cachedContext.Value.Config.SerializationPolicy = SerializationPolicies.Everything;
                UnitySerializationUtility.DeserializeUnityObject(this, ref _serializationData, cachedContext.Value);
            }
        }
        
        void ISerializationCallbackReceiver.OnBeforeSerialize()
        { 
            //Debug.Log("OnBeforeSerialize");
#if UNITY_EDITOR
            Nodes.FindAll(n => n is IReserializable).ConvertAll(n => (IReserializable)n).ForEach(n => n.Reserialize());
            
            using (var cachedContext = OdinSerializer.Utilities.Cache<SerializationContext>.Claim())
            {
                cachedContext.Value.Config.SerializationPolicy = SerializationPolicies.Everything;
                UnitySerializationUtility.SerializeUnityObject(this, ref _serializationData, serializeUnityFields: true, context: cachedContext.Value);
            }
#endif
        }
        
        public byte[] SerializeToBytes(DataFormat p_format, ref List<Object> p_references)
        {
            //Debug.Log("SerializeToBytes "+this);
            byte[] bytes = null;

            using (var cachedContext = Cache<SerializationContext>.Claim())
            {
                cachedContext.Value.Config.SerializationPolicy = SerializationPolicies.Everything;
                UnitySerializationUtility.SerializeUnityObject(this, ref bytes, ref p_references, p_format, true,
                    cachedContext.Value);
            }

            return bytes;
        }

        public void DeserializeFromBytes(byte[] p_bytes, DataFormat p_format, ref List<Object> p_references)
        {
            //Debug.Log("DeserializeToBytes "+this);
            using (var cachedContext = Cache<DeserializationContext>.Claim())
            {
                cachedContext.Value.Config.SerializationPolicy = SerializationPolicies.Everything;
                UnitySerializationUtility.DeserializeUnityObject(this, ref p_bytes, ref p_references, p_format,
                    cachedContext.Value);
            }
        }
        
        public void ValidateSerialization()
        {
            Nodes?.ForEach(n => n.ValidateSerialization());
            UniversalUndo.SetDirty(this);
        }
        
#endregion
        
        public Vector2 viewOffset = Vector2.zero;
        public float zoom = 1;
        
        public virtual void DrawGUI(IViewOwner p_owner, Event p_event, Rect p_rect)
        {
            if (p_owner.GetConfig().deleteNull)
                RemoveNullReferences();
            
            // Draw boxes
            _boxes.Where(r => r != null).ForEach(r => r.DrawGUI(p_owner, this));
            
            // Draw connections
            _connections.Where(c => c != null).ForEach(c=> c.Draw());

            // Draw Nodes
            // Preselect non null to avoid null states from serialization issues
            _nodes.Where(n => n != null).ForEach(n => n.DrawGUI(p_owner, p_rect));

            // Draw user interaction with connections
            NodeConnection.DrawConnectionToMouse(SelectionManager.connectingNode, SelectionManager.connectingIndex, SelectionManager.connectingType, SelectionManager.connectingPosition);

            UniversalUndo.SetDirty(this);
        }
        
        public NodeBase HitsNode(Vector2 p_position)
        {
            return _nodes.AsEnumerable().Reverse().ToList().Find(n => n.rect.Contains(p_position - viewOffset));
        }
        
        public void HitsNode(IViewOwner p_owner, Vector2 p_position, out NodeBase p_node, out ConnectorType p_connectorType, out int p_connectorIndex)
        {
            var reversedNodes = _nodes.AsEnumerable().Reverse().ToList();
            var node = reversedNodes.Find(n => n.rect.Contains(p_position - viewOffset));
            p_connectorType = ConnectorType.INPUT;
            p_connectorIndex = -1;
            
            for (int i = 0; i < reversedNodes.Count; i++)
            {
                p_node = reversedNodes[i];
                p_connectorType = ConnectorType.INPUT;
                p_connectorIndex = reversedNodes[i].HitsConnector(p_owner, p_connectorType, p_position);
                if (p_connectorIndex >= 0)
                    return;
                
                p_connectorType = ConnectorType.OUTPUT;
                p_connectorIndex = reversedNodes[i].HitsConnector(p_owner, p_connectorType, p_position);
                if (p_connectorIndex >= 0)
                    return;
            }

            p_node = node;
        }

        public GraphBox HitsBoxDrag(Vector2 p_position)
        {
            return _boxes.AsEnumerable().Reverse().ToList().Find(r => r.titleRect.Contains(p_position - viewOffset));
        }
        
        public GraphBox HitsBoxResize(Vector2 p_position)
        {
            return _boxes.AsEnumerable().Reverse().ToList().Find(b => b.resizeRect.Contains(p_position - viewOffset));
        }

        public void DeleteBox(GraphBox p_box)
        {
            _boxes.Remove(p_box);
        }
        
        public void CreateBox(Rect p_region)
        {
            // Increase size of region to have padding
            Rect boxRect = new Rect(p_region.xMin - 20, p_region.yMin - 60, p_region.width + 40, p_region.height + 80);
            
            GraphBox box = new GraphBox("Comment", boxRect);
            _boxes.Add(box);
        }

        public NodeConnection HitsConnection(Vector2 p_position, float p_distance)
        {
            return _connections.Find(c => c.Hits(p_position, p_distance));
        }

        public void DrawComments(IViewOwner p_owner, Rect p_rect, bool p_zoomed)
        {
            _nodes.Where(n => n != null).ForEach(n => n.DrawComment(p_owner, p_rect, p_zoomed));
        }
        
        public void RemoveNullReferences()
        {
            Nodes.RemoveAll(n => n == null);
            Connections.RemoveAll(c => c == null);
            Connections.RemoveAll(c => c.inputNode == null || c.outputNode == null);
        }
        
        public void CleanupExposedReferenceTable()
        {
            List<string> exposedGUIDs = new List<string>();
            Nodes.ForEach(n => exposedGUIDs.AddRange(n.GetModelExposedGUIDs()));
            Controller.CleanupReferences(exposedGUIDs);
        }
        
        public T Clone<T>() where T:GraphBase
        {
            List<Object> references = new List<Object>();
            byte[] bytes = this.SerializeToBytes(DataFormat.Binary, ref references);
            T graph = (T) Instantiate(this);

            for (int i = 0; i < references.Count; i++)
            {
                if (references[i] == this)
                    references[i] = graph;
            }
            
            graph.DeserializeFromBytes(bytes, DataFormat.Binary, ref references);
            graph.name = name + "(Clone)";
            return graph;
        }
    }
}