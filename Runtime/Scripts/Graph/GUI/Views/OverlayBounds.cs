/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System.Collections.Generic;
using UnityEngine;

namespace Nodemon
{
    /// <summary>
    /// Frame-scoped registry of overlay-panel bounds. Each overlay view (inspector,
    /// variables, tab panel, etc.) calls <see cref="Register"/> at the end of its
    /// <c>DrawGUI</c> with the rect it actually rendered to. The host editor reads
    /// the registry before iterating views so it can decide whether the mouse is
    /// over an overlay — and if so, mute the background graph view so node-inline
    /// controls don't intercept the click.
    ///
    /// <para>Why a registry rather than a virtual <c>GetOcclusionRect</c> on each
    /// view: the previous formula-based approach lagged or over-claimed (the
    /// variables panel reported its worst-case 3× the empty-panel size, the
    /// inspector's <c>_previousHeight</c> ran a frame behind content growth, etc.).
    /// Bounds posted from the actual drawn rect match what the user sees pixel-
    /// for-pixel, modulo a one-frame lag on the very first MouseDown after
    /// content geometry changes — acceptable in exchange for not having to model
    /// every panel's layout twice.</para>
    ///
    /// <para>Lifecycle within an OnGUI cycle: <see cref="Clear"/> is called by
    /// the host at the start of the Layout event. Views <see cref="Register"/>
    /// during their <c>DrawGUI</c> in that same Layout (and on every subsequent
    /// non-Repaint event, idempotently — last write wins per view). Mouse-event
    /// OnGUI cycles read via <see cref="Query"/> before iterating views. Repaint
    /// events leave the registry alone so it survives until the next Layout.</para>
    /// </summary>
    public static class OverlayBounds
    {
        struct Entry
        {
            public ViewBase View;
            public Rect Bounds;
        }

        // List rather than Dictionary so iteration is in registration order
        // (= draw order = back-to-front z), and Query can walk reverse for the
        // topmost containing entry.
        static readonly List<Entry> _entries = new List<Entry>();

        public static void Clear() => _entries.Clear();

        /// <summary>Records (or replaces) the bounds posted by <paramref name="view"/>.
        /// Calling twice in a frame updates in place rather than duplicating, so
        /// it's safe to call from any non-Repaint event.</summary>
        public static void Register(ViewBase view, Rect bounds)
        {
            for (int i = 0; i < _entries.Count; i++)
            {
                if (_entries[i].View == view)
                {
                    _entries[i] = new Entry { View = view, Bounds = bounds };
                    return;
                }
            }
            _entries.Add(new Entry { View = view, Bounds = bounds });
        }

        /// <summary>Topmost registered view (last entry in z-order) whose bounds
        /// contain <paramref name="mousePos"/>, or null if none.</summary>
        public static ViewBase Query(Vector2 mousePos)
        {
            for (int i = _entries.Count - 1; i >= 0; i--)
            {
                if (_entries[i].Bounds.Contains(mousePos))
                    return _entries[i].View;
            }
            return null;
        }
    }
}
