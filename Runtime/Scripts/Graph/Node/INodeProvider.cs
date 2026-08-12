/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System.Collections.Generic;

namespace Nodemon
{
    /// <summary>
    /// Supplies a graph's node types as data instead of as C# classes.
    ///
    /// <para>A <see cref="GraphBase"/> that returns one of these from
    /// <see cref="GraphBase.NodeProvider"/> is saying "ask me what nodes exist" — the add
    /// menu is built from <see cref="Descriptors"/> and instances come from
    /// <see cref="CreateNode"/>. A graph that returns null keeps the reflection path
    /// unchanged, which is every existing tool.</para>
    ///
    /// <para>The point is a single source of truth. When a node's behaviour lives outside
    /// C# — in a native core — the C# class beside it can only restate what the core
    /// already knows, and a restatement drifts. One class implementing this replaces one
    /// class per node type, and the description comes from wherever the behaviour is.</para>
    ///
    /// <para>Implementations are expected to be cheap to enumerate: the menu asks on every
    /// open. Cache whatever you parse.</para>
    /// </summary>
    public interface INodeProvider
    {
        /// <summary>Every node type this graph can contain, hidden ones included —
        /// filtering is the menu's job, not the provider's.</summary>
        IEnumerable<NodeDescriptor> Descriptors { get; }

        /// <summary>Look one up by its stable id; null when unknown, which is what a
        /// graph saved against a newer core looks like.</summary>
        NodeDescriptor Get(string p_typeId);

        /// <summary>Build an instance for <paramref name="p_descriptor"/>, already
        /// carrying its type id. The provider owns this because the concrete C# class
        /// is its business — typically ONE class parameterised by type id rather than
        /// one class per node.
        ///
        /// <para>Returning null is a refusal the caller must tolerate.</para></summary>
        NodeBase CreateNode(GraphBase p_graph, NodeDescriptor p_descriptor);

        /// <summary>Called after <paramref name="p_node"/> has been positioned and added
        /// to the graph — the provider's chance to finish a creation that is bigger than
        /// one node.
        ///
        /// <para>Some node types are AUTHORED as a pair (a loop's begin and end, in
        /// Houdini terms), and only the provider knows which — the menu hands over one
        /// descriptor and cannot. It runs post-placement because a second node wants a
        /// position relative to the first, and the first has none until the caller sets
        /// it. Default is a no-op, which is the correct behaviour for every solitary
        /// node type.</para></summary>
        void PostCreate(GraphBase p_graph, NodeBase p_node, UnityEngine.Vector2 p_position) { }
    }
}
