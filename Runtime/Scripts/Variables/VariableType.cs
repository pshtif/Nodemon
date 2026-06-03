/*
 *	Created by:  Peter @sHTiF Stefcek
 */

namespace Nodemon
{
    public enum VariableType
    {
        /// <summary>Stored value, editable in the panel. The default mode.</summary>
        VALUE,

        /// <summary>Live-bound to a Unity component's property or field
        /// (see <see cref="Variable.BindProperty"/> / <see cref="Variable.BindField"/>).
        /// Getter/setter dispatch to the component at runtime.</summary>
        BOUND,

        /// <summary>Resolved at initialization from a scene hierarchy
        /// search (see <see cref="Variable.InitializeLookup"/>).</summary>
        LOOKUP,

        /// <summary>Host-supplied graph input. The variable's stored
        /// value is the default; at resolve time, the host
        /// (<c>IGraphController.TryResolveInputBinding</c>) overrides if
        /// it has a binding for the variable's name. Used to declare the
        /// graph's parameter interface — analogous to Houdini HDA's
        /// promoted parameters.</summary>
        INPUT,
    }
}