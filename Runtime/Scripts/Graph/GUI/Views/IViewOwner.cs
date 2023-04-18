/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using UnityEngine;

namespace Nodemon
{
    public interface IViewOwner
    {
        void SetDirty(bool p_dirty);

        GUISkin GetSkin();
        
        ViewConfig GetConfig();

        Camera GetSceneCamera();

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