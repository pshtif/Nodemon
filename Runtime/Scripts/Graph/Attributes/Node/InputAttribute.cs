/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System;

namespace Nodemon.Attributes
{
    /// <summary>
    /// Per-input semantic metadata for a node. Drives the input-connector
    /// hover tooltip (so the user sees "Heightfield" vs "Geometry"
    /// instead of guessing which dot does what) and the cook-time input
    /// validation (when <see cref="Required"/> is true and the input is
    /// missing, or when <see cref="Expects"/> is set and the connected
    /// flow doesn't carry that attribute, the node flags an execution
    /// error with a clear message).
    ///
    /// <para>Apply once per input index on the node class. The number of
    /// inputs themselves is still declared by <see cref="InputCountAttribute"/>;
    /// this attribute is purely semantic and entirely optional — nodes
    /// without it keep their previous behaviour (un-named inputs, no
    /// validation).</para>
    /// </summary>
    /// <example>
    /// <code>
    /// [InputCount(2)]
    /// [Input(0, "Heightfield", Description = "Surface to project onto.", Expects = "heightfield")]
    /// [Input(1, "Geometry",    Description = "Points / mesh to drape.",  Expects = "geometry")]
    /// public class HeightFieldProjectNode { ... }
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class InputAttribute : Attribute
    {
        /// <summary>0-based input port index.</summary>
        public int Index { get; }

        /// <summary>Short human-readable label (e.g. "Heightfield",
        /// "Geometry"). Shown verbatim in the hover tooltip.</summary>
        public string Name { get; }

        /// <summary>Optional one-line description shown under the name in
        /// the hover tooltip. Use to clarify what KIND of geometry / role
        /// the input plays.</summary>
        public string Description { get; set; }

        /// <summary>Optional name of the flow-data attribute the input is
        /// expected to carry (e.g. <c>"geometry"</c>, <c>"heightfield"</c>).
        /// When set, cook-time validation flags an error if the connected
        /// input doesn't have that attribute. Empty / null = no
        /// type expectation.</summary>
        public string Expects { get; set; }

        /// <summary>When true (the default), a missing connection flags
        /// the node with an execution error. Set false for inputs whose
        /// absence is a valid operating mode (e.g. an optional mask
        /// input).</summary>
        public bool Required { get; set; } = true;

        public InputAttribute(int index, string name)
        {
            Index = index;
            Name  = name;
        }
    }
}
