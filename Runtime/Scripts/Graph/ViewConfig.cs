/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Nodemon
{
    [Serializable]
    public class ViewConfig
    {
        public IGraphController editingController;

        public bool HasValidEditingController()
        {
            return !(editingController == null || (editingController is Object && (Object)editingController == null));
        }
        
        [SerializeField]
        public GraphBase editingGraph;
        
        [SerializeField]
        private GraphBase _editingRootGraph;
        
        public GraphBase editingRootGraph
        {
            get { return _editingRootGraph; }
            set { _editingRootGraph = value; }
        }

        public string editingGraphPath;
        
        public bool autoInvalidate = true;
        
        public bool showExperimental = false;
        
        public bool showNodeIds = false;
        
        public bool deleteNull;
    }
}