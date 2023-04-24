/*
 *	Created by:  Peter @sHTiF Stefcek
 */


using System.Collections.Generic;
using Nodemon.Attributes;
using OdinSerializer;
using UnityEngine;

namespace Nodemon
{
    public class SubGraphNodeModel<T> : NodeModelBase where T : GraphBase
    {
        [HideInInspector]
        public bool useAsset = false;
        
        [Dependency("useAsset", true)]
        public T graphAsset;
        
        private int _selfReferenceIndex = -1;
        private byte[] _boundSubGraphData;
        private List<Object> _boundSubGraphReferences;
        
        public T InstanceBoundGraph()
        {
            T graph = ScriptableObject.CreateInstance<T>();
            
            // Empty graphs don't self reference
            if (_selfReferenceIndex != -1)
            {
                _boundSubGraphReferences[_selfReferenceIndex] = graph;
                graph.DeserializeFromBytes(_boundSubGraphData, DataFormat.Binary, ref _boundSubGraphReferences);
            }

            graph.name = id;
            return graph;
        }

        public void Reserialize(GraphBase p_graph)
        {
            if (p_graph != null)
            {
                _boundSubGraphData = p_graph.SerializeToBytes(DataFormat.Binary, ref _boundSubGraphReferences);
                _selfReferenceIndex = _boundSubGraphReferences.FindIndex(r => r == p_graph);
            }
        }
        
        public void SaveToAsset(T p_graph)
        {
#if UNITY_EDITOR
            T graph = GraphUtils.CreateGraphAsAssetFile(p_graph);
            if (graph != null)
            {
                useAsset = true;
                graphAsset = graph;
                
                _selfReferenceIndex = -1;
                _boundSubGraphData = null;
                _boundSubGraphReferences.Clear();
            }
#endif
        }
        
        public void BindToModel()
        {
            _boundSubGraphData = graphAsset.SerializeToBytes(DataFormat.Binary, ref _boundSubGraphReferences);
            _selfReferenceIndex = _boundSubGraphReferences.FindIndex(r => r == graphAsset);

            useAsset = false;
            graphAsset = null;
        }
    }
}