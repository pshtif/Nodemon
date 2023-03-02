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
        
        object Resolve(string p_name, bool p_referenced, IAttributeDataCollection p_collection = null);
    }
}