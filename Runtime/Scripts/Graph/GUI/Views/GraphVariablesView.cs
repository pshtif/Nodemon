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

            DrawVariablesGUI(new Vector2(20,30), false, Color.white, Graph.variables, ref Graph.variablesViewMinimized,
                Graph.Controller == null ? null : Graph.Controller);
        }
    }
}