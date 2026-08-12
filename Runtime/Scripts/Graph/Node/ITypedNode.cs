/*
 *	Created by:  Peter @sHTiF Stefcek
 */

namespace Nodemon
{
    /// <summary>
    /// A node that knows its own type id, because its C# class does not identify it.
    ///
    /// <para>Reflection-discovered nodes are identified BY their class — one class per
    /// node type, and the type is the identity. A provider-supplied node is typically one
    /// class serving every type, so identity has to be carried rather than inferred.</para>
    ///
    /// <para>Deliberately not on <see cref="NodeBase"/>: existing tools identify nodes by
    /// class and gain nothing from this, and adding a field to the base would touch the
    /// serialised shape of every graph in every project that uses Nodemon.</para>
    /// </summary>
    public interface ITypedNode
    {
        /// <summary>Stable id matching a <see cref="NodeDescriptor.TypeId"/> — what a
        /// saved graph stores, and what survives renaming the C# class.</summary>
        string TypeId { get; }
    }
}
