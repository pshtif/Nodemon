/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using Nodemon;
using OdinSerializer.Utilities;
using UnityEngine;

namespace Nodemon
{
    public abstract class ViewBase
    {
        public GraphBase Graph => Owner.GetConfig().editingGraph;
        public IGraphController Controller => Owner.GetConfig().editingController;
        
        public IViewOwner Owner { get; private set; }

        public string name
        {
            get
            {
                return GetType().Name.Substring(0, GetType().Name.IndexOf("View"));
            }
        }
        
        public void SetOwner(IViewOwner p_owner)
        {
            Owner = p_owner;
        }

        public abstract void DrawGUI(Event p_event, Rect p_rect);
        
        public void DrawBoxGUI(Rect p_rect, string p_title, TextAnchor p_titleAlignment, Color p_color)
        {
            GUIStyle style = Owner.GetSkin().GetStyle("ViewBase");
            style.alignment = p_titleAlignment;
            style.richText = true;
            
            switch (p_titleAlignment)
            {
                case TextAnchor.UpperLeft:
                    style.contentOffset = new Vector2(10,0);
                    break;
                case TextAnchor.UpperRight:
                    style.contentOffset = new Vector2(-10,0);
                    break;
                default:
                    style.contentOffset = Vector2.zero;
                    break;
            }

            GUI.color = p_color;
            GUI.Box(p_rect, "", style);
            GUI.Box(new Rect(p_rect.x, p_rect.y, p_rect.width, 32), p_title, style);
            GUI.color = Color.white;
        }
        
        public virtual void ProcessMouse(Event p_event, Rect p_rect) { }

        protected void UseEvent(Rect p_rect)
        {
            if (p_rect.Contains(Event.current.mousePosition) &&
                Event.current.isMouse)
            {
                Event.current.type = EventType.Used;
            }
        }
    }
}