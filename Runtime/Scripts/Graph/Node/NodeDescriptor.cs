/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using UnityEngine;

namespace Nodemon
{
    /// <summary>
    /// A node type described as DATA rather than as a C# class.
    ///
    /// <para>Nodemon normally discovers node types by reflecting over subclasses of
    /// <see cref="NodeBase"/> and reading their attributes. That works when the node's
    /// behaviour lives in C#. It does not work when the behaviour lives elsewhere — a
    /// native core, say — because then the C# class carries nothing but a restatement of
    /// facts the core already knows, and the two drift.</para>
    ///
    /// <para>A graph that can describe its node types supplies these instead (see
    /// <see cref="INodeProvider"/>). Everything here is what the ADD MENU and the node
    /// box need; parameters are described separately by the provider, because a
    /// descriptor is consulted before any node exists.</para>
    ///
    /// <para>Nothing about this is Machina-specific. Any tool whose node types are
    /// described by something other than C# attributes can supply them the same way.</para>
    /// </summary>
    public sealed class NodeDescriptor
    {
        /// <summary>Stable identifier — what a saved graph stores. Never localised,
        /// never derived from a C# type name.</summary>
        public string TypeId;

        /// <summary>Human name for the node box.</summary>
        public string Label;

        /// <summary>Add-menu group. Empty falls back to "Other", matching the
        /// reflection path's behaviour for a node with no category.</summary>
        public string Category;

        /// <summary>Add-menu label when it should differ from <see cref="Label"/>
        /// — null to use the label.</summary>
        public string ContextName;

        /// <summary>Ports a freshly created node shows.</summary>
        public int Inputs;

        /// <summary>Fewer connected inputs than this and the node cannot produce
        /// output. Usually equal to <see cref="Inputs"/>.</summary>
        public int MinInputs;

        /// <summary>Most inputs the node may have; <c>-1</c> is unbounded, and the
        /// editor keeps offering another port as they are wired.</summary>
        public int MaxInputs = -1;

        /// <summary>Per-port: does this port accept more than one connection?
        /// Length matches <see cref="Inputs"/>. Null means every port takes one.
        ///
        /// <para>Load-bearing. A port that silently accepts one wire where the node
        /// expects several drops connections at author time and leaves a graph that
        /// looks right and computes something else.</para></summary>
        public bool[] InputAllowMultiple;

        /// <summary>Optional per-port names. Shorter than <see cref="Inputs"/> (or
        /// null) leaves the tail unnamed.</summary>
        public string[] InputLabels;

        public int Outputs = 1;

        /// <summary>Keep it out of the add menu.</summary>
        public bool Hidden;

        /// <summary>False = at most one of these per graph.</summary>
        public bool AllowMultipleInstances = true;

        /// <summary>Node box size in points; <see cref="Vector2.zero"/> uses the
        /// editor default.</summary>
        public Vector2 Size;

        /// <summary>The node produces nothing until the host binds something to it.
        /// An editor can say so instead of leaving the user to wonder why everything
        /// downstream is empty.</summary>
        public bool HostBound;

        /// <summary>Add-menu tooltip.</summary>
        public string Tooltip;

        /// <summary>Menu path, honouring <see cref="ContextName"/>.</summary>
        public string MenuPath
        {
            get
            {
                string category = string.IsNullOrEmpty(Category) ? "Other" : Category;
                string name = string.IsNullOrEmpty(ContextName) ? Label : ContextName;
                return category + "/" + name;
            }
        }

        /// <summary>Whether input port <paramref name="p_index"/> takes several wires.</summary>
        public bool AllowsMultipleOn(int p_index)
        {
            if (InputAllowMultiple == null || p_index < 0 || p_index >= InputAllowMultiple.Length)
                return false;
            return InputAllowMultiple[p_index];
        }
    }
}
