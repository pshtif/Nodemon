/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniversalGUI;

namespace Nodemon
{
    public class NodeFlowData<T> : IAttributeDataCollection<T>, IEnumerable where T : DataAttribute
    {
        static readonly public NodeFlowData<T>[] EMPTY_ARRAY = {};
        static readonly public NodeFlowData<T> EMPTY = new NodeFlowData<T>();

        protected Dictionary<string, T> _attributes;

        public NodeFlowData()
        {
            _attributes = new Dictionary<string, T>();
        }

        public void BindFromDictionary(Dictionary<string, T> p_properties)
        {
            _attributes = new Dictionary<string, T>(p_properties);
        }

        public bool HasAttribute(string p_name)
        {
            return !p_name.IsNullOrWhitespace() && _attributes.ContainsKey(p_name) && _attributes[p_name] != null;
        }

        public Type GetAttributeType(string p_name)
        {
            if (HasAttribute(p_name))
            {
                return _attributes[p_name].value.GetType();
            }

            return null;
        }

        public void RemoveAttribute(string p_name)
        {
            _attributes.Remove(p_name);
        }

        public K GetAttributeValue<K>(string p_name)
        {
            if (HasAttribute(p_name))
            {
                return (K)_attributes[p_name].value;
            }

            return default(K);
        }
        
        public object GetAttributeValue(string p_name)
        {
            if (HasAttribute(p_name))
            {
                return _attributes[p_name].value;
            }

            return null;
        }

        public T GetAttribute(string p_name)
        {
            return _attributes.ContainsKey(p_name) ? _attributes[p_name] : null;
        }

        public void SetAttribute(T p_attribute)
        {
            _attributes[p_attribute.name] = p_attribute;
        }

        public virtual NodeFlowData<T> Clone()
        {
            var nfd = new NodeFlowData<T>();
            nfd.BindFromDictionary(CloneAttributes(_attributes));
            
            return nfd;
        } 
        
        public Dictionary<string, T>.Enumerator GetEnumerator()
        {
            return _attributes.GetEnumerator();
        }
        
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_attributes.Values).GetEnumerator();
        }
        
        public List<T> GetNameSortedAttributes()
        {
            var sortedList = new List<T>(_attributes.Values);
            sortedList.Sort(delegate(T p_attribute1, T p_attribute2)
            {
                return p_attribute1.name.CompareTo(p_attribute2.name);
            });
            return sortedList;
        }
        
        public static Dictionary<string, T> CloneAttributes(Dictionary<string, T> p_original) 
        {
            Dictionary<string, T> clone = new Dictionary<string, T>(p_original.Count, p_original.Comparer);
            foreach (KeyValuePair<string, T> pair in p_original)
            {
                clone.Add(pair.Key, (T)pair.Value.Clone());
            }
            return clone;
        }
    }
}