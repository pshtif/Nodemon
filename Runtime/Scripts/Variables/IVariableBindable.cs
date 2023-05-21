/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using UnityEngine;

namespace Nodemon
{
    public interface IVariableBindable
    {
        void MarkDirty();
        
        GameObject gameObject { get; }
    }
}