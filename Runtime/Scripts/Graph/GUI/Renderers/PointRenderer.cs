/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System;
using UnityEngine;
using UniversalGUI;

namespace Nodemon
{
    public class PointRenderer
    {
        private static Material _pointMaterial;
        private static Texture2D _pointTexture;
        
        private static Rect GetClippingRect()
        {
            Rect clippingRect = new Rect(0, 0, Screen.width, Screen.height);
            return clippingRect;
        }
        
        private static void SetupPointMaterial(Texture p_texture, Color p_color) 
        {
            if (_pointMaterial == null)
            {
                Shader lineShader = Shader.Find("Hidden/Nodemon/PointShader");
                if (lineShader == null)
                {
                    throw new Exception("LineShader is missing.");
                }

                _pointMaterial = new Material(lineShader);
            }
            
            if (p_texture == null)
            {
                p_texture = _pointTexture != null
                    ? _pointTexture
                    : _pointTexture = TextureUtils.GetTexture("point") as Texture2D;
            }

            _pointMaterial.SetTexture ("_PointTexture", p_texture);
            _pointMaterial.SetColor ("_PointColor", p_color);
            _pointMaterial.SetPass (0);
        }
        
        private static void DrawLineSegment(Vector2 p_point, Vector2 p_perpendicular) 
        {
            GL.TexCoord2 (0, 0);
            GL.Vertex (p_point - p_perpendicular);
            GL.TexCoord2 (0, 1);
            GL.Vertex (p_point + p_perpendicular);
        }
        
        public static void DrawPoint(Vector2 p_position, Color p_color, Texture2D p_texture, float p_width = 1)
        {
            if (Event.current.type != EventType.Repaint)
                return;
            
            SetupPointMaterial (p_texture, p_color);
            GL.Begin (GL.TRIANGLE_STRIP);
            GL.Color (Color.white);

            Rect clippingRect = GetClippingRect();
            
            clippingRect.x = clippingRect.y = 0;
            
            // Removed clipping for testing now
            //if (clippingRect.Contains(p_position))
            {
                GL.TexCoord2 (0, 0);
                GL.Vertex (p_position + new Vector2(-p_width/2, -p_width/2));
                GL.TexCoord2 (0, 1);
                GL.Vertex (p_position + new Vector2(-p_width/2, p_width/2));
                GL.TexCoord2 (1, 0);
                GL.Vertex (p_position + new Vector2(p_width/2, -p_width/2));
                GL.TexCoord2 (1, 1);
                GL.Vertex (p_position + new Vector2(p_width/2, p_width/2));
            }
            
            GL.End ();
        }
    }
}