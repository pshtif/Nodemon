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

        /// <summary>
        /// Resolve a host-supplied input binding by name. The graph's
        /// top-level Input nodes call this at cook time to fetch the
        /// concrete value the host has bound to a named slot (e.g. a
        /// scene MeshFilter, a Texture2D, a scalar). Returns false when
        /// no binding exists for the given name — the caller decides
        /// whether to fall back to a default or fail.
        ///
        /// <para>Host-agnostic on purpose: the return type is <c>object</c>
        /// so any host can return its own concrete shape (Unity refs,
        /// file paths, in-memory handles, etc.). Each InputNode subtype
        /// owns the cast to its expected type.</para>
        /// </summary>
        bool TryResolveInputBinding(string p_name, out object p_value);
    }
}