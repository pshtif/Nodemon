/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using UnityEngine;
using UniversalGUI;
using GUI = UnityEngine.GUI;

namespace Nodemon
{
    public class GraphViewMenu
    {
        private GraphView _view;
        
        public GraphViewMenu(GraphView p_view)
        {
            _view = p_view;
        }
        
        public void Draw(GraphBase p_graph)
        {
            if (p_graph != null)
            {
                if (GUI.Button(new Rect(0, 1, 100, 22), "File"))
                {
                    _view.Owner.GraphFileContextMenu.Show(p_graph);
                }

                GUI.DrawTexture(new Rect(80, 6, 10, 10), TextureUtils.GetTexture("Icons/arrowdown_icon"));

                if (GUI.Button(new Rect(102, 1, 120, 22), "Preferences"))
                {
                    _view.Owner.PreferencesContextMenu.Show(p_graph, _view.Owner.GetConfig());
                }
                
                GUI.DrawTexture(new Rect(202, 6, 10, 10), TextureUtils.GetTexture("Icons/arrowdown_icon"));
            }
        }
    }
}