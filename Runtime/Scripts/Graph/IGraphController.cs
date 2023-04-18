/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System.Collections.Generic;
using UnityEngine;

namespace Nodemon
{
    public interface IGraphController : IVariableBindable
    {
        Variables Variables { get; }
        
        GraphBase Graph { get; }

        Mesh GetCachedMesh();

        void CleanupReferences(List<string> p_guids);
        
        string name { get; }
    }
}