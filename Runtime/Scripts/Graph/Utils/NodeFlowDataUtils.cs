/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System.Collections.Generic;
using UnityEngine;

namespace Nodemon
{
    public class NodeFlowDataUtils
    {
        static public NodeFlowData Create()
        {
            return new NodeFlowData();
        }

        static public T Create<T>() where T : NodeFlowData, new()
        {
            return new T();
        }

        static public T Create<T>(Dictionary<string, object> p_properties) where T : NodeFlowData, new()
        {
            var ndf = new T();
            ndf.BindFromDictionary(p_properties);
            return ndf;
        }
    }
}