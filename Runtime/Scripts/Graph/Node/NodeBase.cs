/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System;
using System.Collections.Generic;
using System.Linq;
using Nodemon.Attributes;
using UnityEngine;
using UniversalGUI;
using GUI = UnityEngine.GUI;
using IconAttribute = UnityEngine.IconAttribute;
using Object = System.Object;
using Random = System.Random;


namespace Nodemon
{
    [Serializable]
    public abstract class NodeBase 
    {
        [NonSerialized] 
        public bool hasErrorsInExecution = false;
        [NonSerialized] 
        public string errorInExecutionMessage = "";
        
        [NonSerialized] 
        private bool _attributesInitialized = false;

        [NonSerialized]
        protected IParameterResolver _parameterResolver;

        public abstract IParameterResolver GetParameterResolver();

        static public NodeBase Create(Type p_nodeType, GraphBase p_graph)
        {
            NodeBase node = (NodeBase)Activator.CreateInstance(p_nodeType);
            node._graph = p_graph;
            node.CreateModel();

            return node;
        }

        [SerializeField]
        protected NodeModelBase _model;

        public bool HasModel()
        {
            return _model != null;
        }

        public NodeModelBase GetModel()
        {
            return _model;
        }
        
        public string Id => _model.id;

        public Random random { get; protected set; }

        [SerializeField] 
        protected GraphBase _graph;

        public GraphBase Graph => _graph;
        
        public IGraphController Controller => Graph.Controller;

        public bool IsDirty { get; protected set; } = true;
        
        [NonSerialized] 
        protected int _executionCounter = 0;

        public bool IsExecuting => _executionCounter > 0;

        [NonSerialized] 
        private int _inputCount;
        public virtual int InputCount
        {
            get
            {
                if (!_attributesInitialized)
                    InitializeAttributes();

                return _inputCount;
            }
        }
        
        [NonSerialized] 
        private bool[] _inputsAllowMultiple;
        public bool[] InputsAllowMultiple
        {
            get
            {
                if (!_attributesInitialized)
                    InitializeAttributes();

                return _inputsAllowMultiple;
            }
        }

        public bool GetInputAllowMultiple(int p_index)
        {
            if (p_index >= InputsAllowMultiple.Length)
                return false;

            return InputsAllowMultiple[p_index];
        }

        [NonSerialized]
        private string[] _outputs;
        public string[] Outputs
        {
            get
            {
                if (!_attributesInitialized)
                    InitializeAttributes();

                return _outputs;
            }
        }
        
        [NonSerialized] 
        private int _outputCount;
        public virtual int OutputCount
        {
            get
            {
                if (!_attributesInitialized)
                    InitializeAttributes();

                return _outputCount;
            }
        }

        public virtual void Initialize() { }

        public virtual void Remove() { }

        protected bool IsException(IAttributeDataCollection p_flowData, string p_variableName, bool p_endsInError = true)
        {
            if (!p_flowData.HasAttribute(p_variableName))
            {
                if (p_endsInError)
                {
                    ThrowException("Variable " + p_variableName + " not found during execution");
                }

                return true;
            }

            return false;
        }

        protected void ThrowException(string p_error)
        {
            Debug.LogWarning(p_error + " : " + _model.id);
            hasErrorsInExecution = true;
        }

        protected bool IsException(Object p_object, bool p_endsInError = true)
        {
            if (p_object == null)
            {
                if (p_endsInError)
                {
                    ThrowException("Object is null during execution");
                }

                return true;
            }

            return false;
        }
        
        public abstract void CreateModel();
        
        public abstract void RecreateModel();
        
        protected void InitializeAttributes()
        {
            InputCountAttribute inputAttribute = (InputCountAttribute) Attribute.GetCustomAttribute(GetType(), typeof(InputCountAttribute));
            _inputCount = inputAttribute == null ? 0 : inputAttribute.count;
            
            InputAllowMultipleAttribute inputAllowMultipleAttribute = (InputAllowMultipleAttribute) Attribute.GetCustomAttribute(GetType(), typeof(InputAllowMultipleAttribute));
            _inputsAllowMultiple = inputAllowMultipleAttribute == null ? new bool[0] : inputAllowMultipleAttribute.allowMultiple;
            
            OutputLabelsAttribute outputLabelsAttribute = (OutputLabelsAttribute) Attribute.GetCustomAttribute(GetType(), typeof(OutputLabelsAttribute));
            _outputs = outputLabelsAttribute == null ? new string[0] : outputLabelsAttribute.labels;
            
            OutputCountAttribute outputAttribute = (OutputCountAttribute) Attribute.GetCustomAttribute(GetType(), typeof(OutputCountAttribute));
            _outputCount = outputAttribute == null ? 0 : outputAttribute.count;

            if (Attribute.GetCustomAttribute(GetType(), typeof(CustomToolAttribute)) != null)
                _usesCustomTool = true;
            
            SkinAttribute skinAttribute = (SkinAttribute) Attribute.GetCustomAttribute(GetType(), typeof(SkinAttribute));
            _backgroundSkinId = skinAttribute != null ? skinAttribute.backgroundSkinId : "NodeBody";
            
            SizeAttribute sizeAttribute = (SizeAttribute) Attribute.GetCustomAttribute(GetType(), typeof(SizeAttribute));
            _size = sizeAttribute != null ? new Vector2(sizeAttribute.width, sizeAttribute.height) : Vector2.one;

            InitializeCustomAttributes();

            _attributesInitialized = true;
        }

        protected abstract void InitializeCustomAttributes();

        protected T GetParameterValue<T>(Parameter<T> p_parameter) 
        {
            if (p_parameter == null)
                return default(T);
            
            T value = p_parameter.GetValue(GetParameterResolver());
            if (!hasErrorsInExecution && p_parameter.hasErrorInEvaluation)
            {
                SetError(p_parameter.errorMessage);
            }
            hasErrorsInExecution = hasErrorsInExecution || p_parameter.hasErrorInEvaluation;
            return value;
        } 
        
        protected object GetParameterValue<K>(Parameter p_parameter, Type p_parameterType, NodeFlowData<K> p_flowData = null, int p_index = 0) where K : DataAttribute
        {
            if (p_parameter == null)
                return null;
            
            object value = p_parameter.GetValue(GetParameterResolver(), p_parameterType, p_flowData, p_index);
            if (!hasErrorsInExecution && p_parameter.hasErrorInEvaluation)
            {
                SetError(p_parameter.errorMessage);
            }
            hasErrorsInExecution = hasErrorsInExecution || p_parameter.hasErrorInEvaluation;
            return value;
        }
        
        protected T GetParameterValue<T, K>(Parameter<T> p_parameter, NodeFlowData<K> p_flowData = null, int p_index = 0) where K : DataAttribute
        {
            if (p_parameter == null)
                return default(T);
            
            T value = p_parameter.GetValue(GetParameterResolver(), p_flowData, p_index);
            if (!hasErrorsInExecution && p_parameter.hasErrorInEvaluation)
            {
                SetError(p_parameter.errorMessage);
            }
            hasErrorsInExecution = hasErrorsInExecution || p_parameter.hasErrorInEvaluation;
            return value;
        }
        
        protected void SetError(string p_warning = null)
        {
            errorInExecutionMessage = p_warning;
            hasErrorsInExecution = true;
        }

        protected bool CheckException(IAttributeDataCollection p_flowData, string p_variableName)
        {
            if (!p_flowData.HasAttribute(p_variableName))
            {
                SetError("Variable "+p_variableName+" not found during execution of "+this);

                return true;
            }

            return false;
        }

        protected bool CheckException(Object p_object, string p_warning = null)
        {
            if (p_object == null)
            {
                SetError(p_warning);
                
                return true;
            }

            return false;
        }

        public NodeBase Clone(GraphBase p_graph)
        {
            NodeBase node = Create(GetType(), p_graph);
            node._model = _model.Clone(Controller as IExposedPropertyTable);
            node.ValidateUniqueId();
            return node;
        }
        
        protected void ValidateUniqueId()
        {
            string id = _model.id;
            if (string.IsNullOrEmpty(id))
            {
                string type = GetType().ToString();
                id = type.Substring(5, type.Length-9) + "1";
            }

            while (Graph.Nodes.Exists(n => n != this && n.Id == id))
            {
                string number = string.Concat(id.Reverse().TakeWhile(char.IsNumber).Reverse());
                id = id.Substring(0,id.Length-number.Length) + (Int32.Parse(number)+1);
            }
            
            _model.id = id;
        }
        
        [NonSerialized]
        public float executeTime = 0;

        [NonSerialized] 
        private bool _usesCustomTool = false;
        public bool UsesCustomTool 
        {
            get
            {
                if (!_attributesInitialized)
                    InitializeAttributes();

                return _usesCustomTool;
            }
        }

        [NonSerialized] 
        private string _backgroundSkinId;
        public string BackgroundSkinId 
        {
            get
            {
                if (!_attributesInitialized)
                    InitializeAttributes();
                
                return _backgroundSkinId;
            }
        }

        [NonSerialized]
        private Vector2 _size = Vector2.zero;

        public virtual Vector2 Size
        {
            get
            {
                if (!_attributesInitialized)
                    InitializeAttributes();

                return _size;
            }
        }

        [NonSerialized]
        protected Texture _iconTexture;
        
        protected Texture IconTexture
        {
            get
            {
                if (!_attributesInitialized)
                    InitializeAttributes();

                return _iconTexture;
            }
        }

        [NonSerialized]
        protected Color _nodeBackgroundColor = new Color(0.8f, 0.6f, 0f);
        protected virtual Color NodeBackgroundColor
        {
            get
            {
                if (!_attributesInitialized)
                    InitializeAttributes();

                return _nodeBackgroundColor;
            }
        }

        [NonSerialized] 
        protected Color _titleTextColor = Color.white;
        protected virtual Color TitleTextColor
        {
            get
            {
                if (!_attributesInitialized)
                    InitializeAttributes();

                return _titleTextColor;
            }
        }
        
        internal virtual void Unselect() { }

        public int Index => Graph.Nodes.IndexOf(this);

        public Rect rect;

        public virtual string CustomName => String.Empty;

        public string Name => String.IsNullOrEmpty(CustomName)
            ? GetNodeNameFromType(this.GetType())
            : CustomName;

        public static string GetNodeNameFromType(Type p_nodeType)
        {
            string typeString = p_nodeType.ToString();
            int dotIndex = typeString.IndexOf(".");
            return typeString.Substring(dotIndex + 1, typeString.Length-(dotIndex+5));
        }

        public virtual void MarkDirty()
        {
            IsDirty = true;
        }

        // protected virtual bool Invalidate_Internal()
        // {
        //     return false;
        // }

        public void ValidateSerialization()
        {
            _model.ValidateSerialization();
        }

        public virtual void DrawGUI(IViewOwner p_owner, Rect p_viewRect)
        {
            rect = new Rect(rect.x, rect.y, Size.x, Size.y);
            Rect offsetRect = new Rect(rect.x + Graph.viewOffset.x, rect.y + Graph.viewOffset.y, Size.x, Size.y);
            
            if (IsCulled(p_viewRect, offsetRect))
                return;

            DrawConnectors(p_owner, p_viewRect);
            
            GUI.color = NodeBackgroundColor;
            GUI.Box(offsetRect, "", p_owner.GetSkin().GetStyle(BackgroundSkinId));

            DrawTitle(p_owner, offsetRect);

            DrawId(p_owner, offsetRect);

            DrawOutline(p_owner, offsetRect);

            DrawCustomGUI(p_owner, offsetRect);
        }

        protected bool IsCulled(Rect p_viewRect, Rect p_nodeRect)
        {
            if (!p_viewRect.Contains(new Vector2(p_nodeRect.x, p_nodeRect.y)) &&
                !p_viewRect.Contains(new Vector2(p_nodeRect.x + p_nodeRect.width, p_nodeRect.y)) &&
                !p_viewRect.Contains(new Vector2(p_nodeRect.x, p_nodeRect.y + p_nodeRect.height)) &&
                !p_viewRect.Contains(new Vector2(p_nodeRect.x + p_nodeRect.width, p_nodeRect.y + p_nodeRect.height)))
                return true;

            return false;
        }

        protected virtual void DrawNode(IViewOwner p_owner, Rect p_rect)
        {
            GUI.color = NodeBackgroundColor;
            GUI.Box(p_rect, "", p_owner.GetSkin().GetStyle(BackgroundSkinId));
        }

        protected virtual void DrawTitle(IViewOwner p_owner, Rect p_rect)
        {
            GUI.color = TitleTextColor;
            GUI.Label(
                new Rect(new Vector2(p_rect.x + 6, p_rect.y+2),
                    new Vector2(Size.x-12, 20)), Name, p_owner.GetSkin().GetStyle("NodeTitle"));

            if (IconTexture != null)
            {
                GUI.DrawTexture(new Rect(p_rect.x + 6, p_rect.y + 6, 16, 16),
                    IconTexture);
            }
        }

        protected void DrawId(IViewOwner p_owner, Rect p_rect)
        {
            if (!p_owner.GetConfig().showNodeIds)
                return;
            
            GUI.color = Color.gray;
            if (!String.IsNullOrEmpty(_model.id))
            {
                GUI.Label(new Rect(new Vector2(p_rect.x + p_rect.width + 24, p_rect.y + p_rect.height/2 - 10), new Vector2(rect.width - 5, 20)), _model.id);
            }
            GUI.color = Color.white;
        }

        public bool HasComment()
        {
            return _model.comment != null;
        }

        public void CreateComment()
        {
            _model.comment = "Comment";
        }

        public void RemoveComment()
        {
            _model.comment = null;
        }
        
        protected virtual void DrawOutline(IViewOwner p_owner, Rect p_rect)
        {
            if (SelectionManager.IsSelected(Graph, this))
            {
                GUI.color = Color.green;
                GUI.Box(new Rect(p_rect.x - 2, p_rect.y - 2, p_rect.width + 4, p_rect.height + 4),
                    "",  p_owner.GetSkin().GetStyle("NodeSelected"));
            }
            
            if (SelectionManager.IsSelecting(Graph, this))
            {
                GUI.color = Color.yellow;
                GUI.Box(new Rect(p_rect.x - 2, p_rect.y - 2, p_rect.width + 4, p_rect.height + 4),
                    "",  p_owner.GetSkin().GetStyle("NodeSelected"));
            }

            if (hasErrorsInExecution)
            {
                GUI.color = Color.red;
                GUI.Box(new Rect(p_rect.x - 2, p_rect.y - 2, p_rect.width + 4, p_rect.height + 4),
                    "",  p_owner.GetSkin().GetStyle("NodeSelected"));
                if (GUI.Button(new Rect(p_rect.x + 2, p_rect.y - 22, 16, 16),
                        TextureUtils.GetTexture("Icons/error_icon"), GUIStyle.none))
                {
                    Debug.Log("Execution error: " + errorInExecutionMessage);
                }
                // GUI.DrawTexture(new Rect(p_rect.x + 2, p_rect.y - 22, 16, 16),
                //     TextureUtils.GetTexture("Icons/error_icon"));
            }
        }
        
        public void DrawComment(IViewOwner p_owner, Rect p_rect, bool p_zoomed = true)
        {
            if (_model.comment == null)
                return;

            Rect offsetRect = p_zoomed
                ? new Rect(rect.x + Graph.viewOffset.x, rect.y + Graph.viewOffset.y, Size.x, Size.y)
                : new Rect((rect.x + Graph.viewOffset.x) / Graph.zoom, (rect.y + Graph.viewOffset.y) / Graph.zoom, Size.x, Size.y);
            
            GUIStyle commentStyle = new GUIStyle();
            commentStyle.font = p_owner.GetSkin().GetStyle("NodeComment").font;
            commentStyle.fontSize = 14;
            commentStyle.normal.textColor = Color.black;

            string commentText = _model.comment;
            Vector2 size = commentStyle.CalcSize( new GUIContent( commentText ) );
            
            GUI.color = new Color(1,1,1,.6f);
            GUI.Box(new Rect(offsetRect.x - 10, offsetRect.y - size.y - 26, size.x < 34 ? 50 : size.x + 16, size.y + 26), "", p_owner.GetSkin().GetStyle("NodeComment"));
            GUI.color = Color.white;
            string text = GUI.TextArea(new Rect(offsetRect.x - 2, offsetRect.y - size.y - 21, size.x, size.y), commentText, commentStyle);
            _model.comment = text;
        }

        public virtual Rect GetConnectorRect(ConnectorType p_connectorType, int p_index)
        {
            Rect offsetRect = new Rect(rect.x + Graph.viewOffset.x, rect.y + Graph.viewOffset.y, Size.x,
                Size.y);

            int size = 24;
            int padding = 8;
            
            Rect connectorRect;
            if (p_connectorType == ConnectorType.INPUT)
            {
                connectorRect = new Rect(offsetRect.x + offsetRect.width / 2 - InputCount / 2f * size -
                    (InputCount - 1) / 2f * padding + p_index * (size + padding),
                    offsetRect.y - size / 2, size, size);
            }
            else
            {
                connectorRect = new Rect(offsetRect.x + offsetRect.width / 2 - OutputCount / 2f * size -
                    (OutputCount - 1) / 2f * padding + p_index * (size + padding),
                    offsetRect.y + offsetRect.height - size / 2, size, size);
            }

            return connectorRect;
        }
        
        public int HitsConnector(IViewOwner p_owner, ConnectorType p_connectorType, Vector2 p_position)
        {
            int count = p_connectorType == ConnectorType.INPUT ? InputCount : OutputCount;
            for (int i = 0; i < count; i++)
            {
                var connectorRect = GetConnectorRect(p_connectorType, i);
                if (connectorRect.Contains(p_position))
                {
                    return i;
                }
            }

            return -1;
        }
        
        protected void DrawConnectors(IViewOwner p_owner, Rect p_rect)
        {
            Color inputConnectedColor = new Color(0f, 0.7f, 1f);
            Color inputDisconnectedColor = new Color(0.5f, 0.35f, 0f);
            Color outputConnectedColor = new Color(0, 0.7f, 1f);
            Color outputDisconnectedColor = new Color(.5f, .35f, 0);
            
            // Inputs
            for (int i = 0; i < InputCount; i++)
            {
                bool isConnected = Graph.HasInputConnected(this, i);
                GUI.color = isConnected ? inputConnectedColor
                    : inputDisconnectedColor;

                if (IsExecuting)
                    GUI.color = Color.cyan;

                var connectorRect = GetConnectorRect(ConnectorType.INPUT, i);
                GUI.Label(connectorRect, "", p_owner.GetSkin().GetStyle(isConnected ? "NodeConnectorOn" : "NodeConnectorOff"));
            }
            
            // Outputs
            for (int i = 0; i < OutputCount; i++)
            {
                bool isConnected = Graph.HasOutputConnected(this, i); 
                GUI.color = isConnected ? outputConnectedColor
                    : outputDisconnectedColor;

                if (SelectionManager.connectingNode == this && SelectionManager.connectingType == ConnectorType.OUTPUT && SelectionManager.connectingIndex == i)
                    GUI.color = Color.green;

                var connectorRect = GetConnectorRect(ConnectorType.OUTPUT, i);
                
                if (connectorRect.Contains(Event.current.mousePosition - new Vector2(p_rect.x, p_rect.y)))
                    GUI.color = Color.green;

                GUI.Label(connectorRect, "", p_owner.GetSkin().GetStyle(isConnected ? "NodeConnectorOn" : "NodeConnectorOff"));
            }
        }
        
        protected virtual void DrawCustomGUI(IViewOwner p_owner, Rect p_rect) { }

        public void DragNode(Event p_event, Rect p_viewRect, float p_zoom)
        {
            rect.x += p_event.delta.x * p_zoom;
            rect.y += p_event.delta.y * p_zoom;

        }
        
        public virtual void DrawInspector(IViewOwner p_owner)
        {
            bool invalidate = _model.DrawInspector(p_owner);
            invalidate = invalidate || DrawCustomInspector(p_owner);

            if (invalidate)
            {
                UniversalUndo.RegisterCompleteObjectUndo(Graph, "Inspector");
                MarkDirty();
                Graph.MarkDirty();
            }
        }

        public virtual bool DrawCustomInspector(IViewOwner p_owner)
        {
            return false;
        }
        
        public bool IsInsideRect(Rect p_rect, float p_zoom)
        {
            if (p_rect.Contains(new Vector2((rect.x + Graph.viewOffset.x)/p_zoom,
                    (rect.y + Graph.viewOffset.y)/p_zoom)) ||
                p_rect.Contains(new Vector2((rect.x + rect.width + Graph.viewOffset.x)/p_zoom,
                    (rect.y + Graph.viewOffset.y)/p_zoom)) ||
                p_rect.Contains(new Vector2((rect.x + Graph.viewOffset.x)/p_zoom,
                    (rect.y + rect.height + Graph.viewOffset.y)/p_zoom)) ||
                p_rect.Contains(new Vector2((rect.x + rect.width + Graph.viewOffset.x)/p_zoom,
                    (rect.y + rect.height + Graph.viewOffset.y)/p_zoom)))
            {
                return true;
            }

            return false;
        }

        protected virtual void DrawCustomSceneGUI(IViewOwner p_owner) { }

        public List<string> GetModelExposedGUIDs()
        {
            return _model.GetExposedGUIDs();
        }
    }
    
    [Serializable]
    public abstract class NodeBase<T> : NodeBase where T : NodeModelBase, new()
    {
        public T Model => (T)_model;
        
        public override void CreateModel()
        {
            _model = new T();
            _model.id = Graph.GenerateNodeId(this, _model.id);
            random = new System.Random(Mathf.FloorToInt(Time.realtimeSinceStartup));
            _model.seed = new Parameter<int>(random.Next(1000000, 10000000));
        }

        public override void RecreateModel()
        {
            
        }
    }
}