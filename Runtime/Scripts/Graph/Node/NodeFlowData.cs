/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System;
using System.Collections;
using System.Collections.Generic;

namespace Nodemon
{
    public class NodeFlowData : IAttributeDataCollection, IEnumerable
    {
        static readonly public NodeFlowData[] EMPTY_ARRAY = {};
        static readonly public NodeFlowData EMPTY = new NodeFlowData();

        protected Dictionary<string, object> _attributes;

        public NodeFlowData()
        {
            _attributes = new Dictionary<string, object>();
        }
        
        public void BindFromDictionary(Dictionary<string, object> p_properties)
        {
            _attributes = new Dictionary<string, object>(p_properties);
        }

        public bool HasAttribute(string p_name)
        {
            return _attributes.ContainsKey(p_name) && _attributes[p_name] != null;
        }

        public Type GetAttributeType(string p_name)
        {
            if (HasAttribute(p_name))
            {
                return _attributes[p_name].GetType();
            }

            return null;
        }

        public void RemoveAttribute(string p_name)
        {
            _attributes.Remove(p_name);
        }

        public T GetAttribute<T>(string p_name)
        {
            if (HasAttribute(p_name))
            {
                return (T) _attributes[p_name];
            }

            return default(T);
        }

        public object GetAttribute(string p_name)
        {
            return _attributes.ContainsKey(p_name) ? _attributes[p_name] : null;
        }

        public void SetAttribute(string p_name, object p_value)
        {
            if (_attributes.ContainsKey(p_name))
            {
                _attributes[p_name] = p_value;
            }
            else
            {
                _attributes.Add(p_name, p_value);
            }
        }

        public NodeFlowData Clone()
        {
            var ndf = new NodeFlowData();
            ndf.BindFromDictionary(CloneAttributes(_attributes));
            
            return ndf;
        } 
        
        public Dictionary<string, object>.Enumerator GetEnumerator()
        {
            return _attributes.GetEnumerator();
        }
        
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_attributes.Values).GetEnumerator();
        }
        
        public static Dictionary<string, object> CloneAttributes(Dictionary<string, object> p_original) 
        {
            Dictionary<string, object> clone = new Dictionary<string, object>(p_original.Count, p_original.Comparer);
            foreach (KeyValuePair<string, object> pair in p_original)
            {
                if (pair.Value == null || pair.Value.GetType().IsValueType)
                {
                    clone.Add(pair.Key, pair.Value);
                    continue;
                }

                if (pair.Value.GetType() == typeof(int[]))
                {
                    clone.Add(pair.Key, ((int[])pair.Value).Clone());
                    continue;           
                }
                
                // TODO deep cloning for other types
                clone.Add(pair.Key, pair.Value);
            }
            return clone;
        }
    }
    
    // TODO implement pooling
}