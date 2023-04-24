/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using UnityEngine;

namespace Nodemon
{
    public interface IViewOwner
    {
        void SetTooltip(string p_tooltip);
        
        string GetTooltip();
        
        void SetStatus(string p_status);

        string GetStatus();
        
        void SetDirty(bool p_dirty);

        GUISkin GetSkin();
        
        ViewConfig GetConfig();

        Camera GetSceneCamera();

        void Repaint();

        void EditController(IGraphController p_controller, string p_path);

        void EditGraph(GraphBase p_graph, string p_path);

        string GetVersionLabel();

        GraphFileContextMenu GraphFileContextMenu { get; }
        PreferencesContextMenu PreferencesContextMenu { get; }
        CreateNodeContextMenu CreateNodeContextMenu { get; }
        NodeContextMenu NodeContextMenu { get; }
        ConnectionContextMenu ConnectionContextMenu { get; }
        BoxContextMenu BoxContextMenu { get; }
        TypesMenu TypesMenu { get; }
    }
}