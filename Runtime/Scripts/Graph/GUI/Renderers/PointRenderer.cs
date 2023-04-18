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

        private static bool _immediateMode;

        private static void SetupPointMaterial(Color p_color, Texture p_texture) 
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

            _pointMaterial.SetTexture("_PointTexture", p_texture);
            _pointMaterial.SetColor("_PointColor", p_color);
            _pointMaterial.SetPass(0);
        }

        public static void BeginImmediateMode(Color p_color, Texture2D p_texture = null)
        {
            _immediateMode = true;
            SetupPointMaterial(p_color, p_texture);
            GL.Begin(GL.QUADS);
            GL.Color(Color.white);
        }
        
        public static void EndImmediateMode()
        {
            _immediateMode = false;
            GL.End();
        }
        
        public static void DrawPoint(Vector3 p_position, float p_size, Camera p_camera, Color p_color, Texture2D p_texture = null)
        {
            if (!_immediateMode)
            {
                SetupPointMaterial(p_color, p_texture);
                GL.Begin(GL.TRIANGLE_STRIP);
                GL.Color(Color.white);
            }

            var distance = p_camera.transform.position - p_position;
            p_size = p_size * distance.magnitude / 100f;

            GL.TexCoord2 (0, 0);
            GL.Vertex (p_position + p_camera.transform.up * p_size/2 - p_camera.transform.right * -p_size/2);
            GL.TexCoord2 (0, 1);
            GL.Vertex (p_position - p_camera.transform.up * p_size/2 - p_camera.transform.right * -p_size/2);
            GL.TexCoord2 (1, 1);
            GL.Vertex (p_position - p_camera.transform.up * p_size/2 + p_camera.transform.right * -p_size/2);
            GL.TexCoord2 (1, 0);
            GL.Vertex (p_position + p_camera.transform.up * p_size/2 + p_camera.transform.right * -p_size/2);

            if (!_immediateMode)
            {
                GL.End();
            }
        }
    }
}