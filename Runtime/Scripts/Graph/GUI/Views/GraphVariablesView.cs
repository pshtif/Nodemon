/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using UnityEngine;

namespace Nodemon
{
    public class GraphVariablesView : VariablesView
    {
        private Vector2 scrollPosition;

        public override void DrawGUI(Event p_event, Rect p_rect)
        {
            if (Graph == null)
                return;

            DrawVariablesGUI(new Vector2(20, 30), false, Color.white, Graph.variables, ref Graph.variablesViewMinimized,
                Graph.Controller == null ? null : Graph.Controller);

            // Mirror DrawVariablesGUI's own height formula so the registered
            // bounds match the visible panel pixel-for-pixel. Without this,
            // clicks landing in the area between the panel's real bottom and
            // the registry's claimed bottom would still be muted incorrectly.
            int count = Graph.variables.Count;
            int height = Graph.variablesViewMinimized
                ? 32
                : (count <= 10 ? 64 + count * 22 : 64 + 220);
            OverlayBounds.Register(this, new Rect(20, 30, 380, height));
        }
    }
}