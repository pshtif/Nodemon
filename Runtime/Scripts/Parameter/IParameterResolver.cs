/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System;

namespace Nodemon
{
    public interface IParameterResolver
    {
        bool hasErrorInResolving { get; }
        
        string errorMessage { get; }
        
        object Resolve<K>(string p_name, bool p_referenced, IAttributeDataCollection<K> p_collection = null, int p_index = 0) where K : DataAttribute;
    }
}