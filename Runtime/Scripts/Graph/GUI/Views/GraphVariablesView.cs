/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using UnityEngine;

namespace Nodemon
{
    public class GraphVariablesView : VariablesView
    {
        const float PANEL_X    = 20f;
        const float PANEL_Y    = 30f;
        const float PANEL_W    = 380f;
        const float HDR_H      = 32f;        // minimized strip height
        const float MAX_BODY_H = 64 + 220;   // matches DrawVariablesGUI's cap

        private Vector2 scrollPosition;

        public override Rect? GetOcclusionRect(Rect p_canvasRect)
        {
            if (Graph == null) return null;
            float height = Graph.variablesViewMinimized
                ? HDR_H
                : MAX_BODY_H; // worst-case so click area doesn't lag content
            return new Rect(PANEL_X, PANEL_Y, PANEL_W, height);
        }

        public override void DrawGUI(Event p_event, Rect p_rect)
        {
            if (Graph == null)
                return;

            DrawVariablesGUI(new Vector2(PANEL_X, PANEL_Y), false, Color.white, Graph.variables, ref Graph.variablesViewMinimized,
                Graph.Controller == null ? null : Graph.Controller);
        }
    }
}