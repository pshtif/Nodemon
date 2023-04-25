/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System;
using UnityEngine;
using UniversalGUI;

namespace Nodemon
{
    public static class LineRenderer
    {
        private static Material _lineMaterial;
        private static Texture2D _lineTexture;

        public static Rect customClipRect = new Rect(0,0,0,0);
        
        private static Rect GetGUIClippingRect()
        {
            Rect clippingRect;
#if UNITY_EDITOR
            clippingRect = GUIScaleUtils.TopRect;
#else
            if (customClipRect.width == 0 || customClipRect.height == 0)
            {
                clippingRect = new Rect(0, 0, Screen.width * GUIScaleUtils.lastZoom,
                    Screen.height * GUIScaleUtils.lastZoom);
            }
            else
            {
                clippingRect = new Rect(customClipRect.x, customClipRect.y, customClipRect.width * GUIScaleUtils.lastZoom,
                    customClipRect.height * GUIScaleUtils.lastZoom);
            }

#endif
            return clippingRect;
        }

        public static void SetupLineMaterial(Texture p_texture, Color p_color, bool p_zTest) 
        {
            if (_lineMaterial == null)
            {
	            Shader lineShader = p_zTest
		            ? Shader.Find("Hidden/Nodemon/LineShaderZTest")
		            : Shader.Find("Hidden/Nodemon/LineShader");

	            if (lineShader == null)
                {
                    throw new Exception("LineShader is missing.");
                }

                _lineMaterial = new Material(lineShader);
            }
            
            if (p_texture == null)
            {
                p_texture = _lineTexture != null
                    ? _lineTexture
                    : _lineTexture = TextureUtils.GetTexture("line") as Texture2D;
            }

            _lineMaterial.SetTexture("_LineTexture", p_texture);
            _lineMaterial.SetColor("_LineColor", p_color);
            _lineMaterial.SetPass(0);
        }

        public static void DrawGUIBezier(Vector2 p_startPos, Vector2 p_endPos, Vector2 p_startTan, Vector2 p_endTan, Color p_color, Texture2D p_texture, float p_width = 1)
		{
			if (Event.current.type != EventType.Repaint)
				return;
            
            Rect clippingRect = GetGUIClippingRect();
            
			clippingRect.x = clippingRect.y = 0;
			Rect bounds = new Rect(Mathf.Min(p_startPos.x, p_endPos.x), Mathf.Min(p_startPos.y, p_endPos.y), 
								Mathf.Abs(p_startPos.x - p_endPos.x), Mathf.Abs(p_startPos.y - p_endPos.y));
            
			if (!bounds.Overlaps(clippingRect))
				return;
            
			int segmentCount = CalculateBezierSegmentCount(p_startPos, p_endPos, p_startTan, p_endTan);

			DrawGUIBezier(p_startPos, p_endPos, p_startTan, p_endTan, p_color, p_texture, segmentCount, p_width);
		}
        
		public static void DrawGUIBezier(Vector2 p_startPos, Vector2 p_endPos, Vector2 p_startTan, Vector2 p_endTan, Color p_color, Texture2D p_texture, int p_segmentCount, float p_width)
		{
			if (Event.current.type != EventType.Repaint && Event.current.type != EventType.KeyDown)
				return;

            Rect clippingRect = GetGUIClippingRect();
            
			clippingRect.x = clippingRect.y = 0;
			Rect bounds = new Rect(Mathf.Min(p_startPos.x, p_endPos.x), Mathf.Min(p_startPos.y, p_endPos.y), 
								Mathf.Abs(p_startPos.x - p_endPos.x), Mathf.Abs(p_startPos.y - p_endPos.y));
            
			if (!bounds.Overlaps(clippingRect))
				return;
            
			Vector2[] bezierPoints = new Vector2[p_segmentCount+1];
            for (int i = 0; i <= p_segmentCount; i++)
            {
                bezierPoints[i] = GetBezierPoint((float)i / p_segmentCount, p_startPos, p_endPos, p_startTan, p_endTan);
            }

            DrawGUIPolygonLine(bezierPoints, p_color, p_texture, p_width);
		}
        
		public static void DrawGUIPolygonLine(Vector2[] p_points, Color p_color, Texture2D p_texture, float p_width = 1)
		{
			if (Event.current.type != EventType.Repaint && Event.current.type != EventType.KeyDown)
				return;
            
			if (p_points.Length == 1)
				return;

            if (p_points.Length == 2)
            {
                DrawGUILine(p_points[0], p_points[1], p_color, p_texture, p_width);
                return;
            }
            
            SetupLineMaterial(p_texture, p_color, true);
            GL.Begin(GL.TRIANGLE_STRIP);
            GL.Color(Color.white);

            Rect clippingRect = GetGUIClippingRect();
            
			clippingRect.x = clippingRect.y = 0;

            Vector2 currentPoint = p_points[0];
            Vector2 nextPoint;
            Vector2 perpendicular;
			bool point1Clipped, point2Clipped;
            
			for (int pointCnt = 1; pointCnt < p_points.Length; pointCnt++) 
			{
				nextPoint = p_points[pointCnt];
                
				Vector2 curPointOriginal = currentPoint, nextPointOriginal = nextPoint;
				if (SegmentRectIntersection(clippingRect, ref currentPoint, ref nextPoint, out point1Clipped, out point2Clipped))
				{
                    if (pointCnt < p_points.Length - 1)
                    {
                        perpendicular = CalculatePointPerpendicular(curPointOriginal, nextPointOriginal,
                            p_points[pointCnt + 1]);
                    }
                    else
                    {
                        perpendicular = CalculateLinePerpendicular(curPointOriginal, nextPointOriginal);
                    }

                    if (point1Clipped)
                    {
                        Draw2DLineSegment(currentPoint, perpendicular * p_width / 2);
                    }

                    if (pointCnt == 1)
                    {
                        Draw2DLineSegment(currentPoint, CalculateLinePerpendicular(currentPoint, nextPoint) * p_width / 2);
                    }

                    Draw2DLineSegment(nextPoint, perpendicular * p_width/2);
				}

                currentPoint = nextPointOriginal;
			}
            
            GL.End();
        }


		private static int CalculateBezierSegmentCount(Vector2 p_startPos, Vector2 p_endPos, Vector2 p_startTan, Vector2 p_endTan)
        {
            float straightFactor = Vector2.Angle(p_startTan - p_startPos, p_endPos - p_startPos) *
                                   Vector2.Angle(p_endTan - p_endPos, p_startPos - p_endPos) *
                                   (p_endTan.magnitude + p_startTan.magnitude);
			straightFactor = 2 + Mathf.Pow(straightFactor / 400, 0.125f); // 1/8
            
			float distanceFactor = 1 + (p_startPos-p_endPos).magnitude;
            distanceFactor = Mathf.Pow(distanceFactor, 0.1f);
            return 4 + (int)(straightFactor * distanceFactor);
		}
        
		private static Vector2 CalculateLinePerpendicular(Vector2 p_startPos, Vector2 p_endPos) 
		{
			return new Vector2 (p_endPos.y-p_startPos.y, p_startPos.x-p_endPos.x).normalized;
		}
        
		private static Vector2 CalculatePointPerpendicular(Vector2 p_previousPos, Vector2 p_pointPos, Vector2 p_nextPos) 
		{
			return CalculateLinePerpendicular (p_pointPos, p_pointPos + (p_nextPos-p_previousPos));
		}
        
		private static Vector2 GetBezierPoint(float p_t, Vector2 p_startPos, Vector2 p_endPos, Vector2 p_startTan, Vector2 p_endTan) 
		{
			float rt = 1 - p_t;
			float rtt = rt * p_t;

            return p_startPos * rt * rt * rt +
                   p_startTan * 3 * rt * rtt +
                   p_endTan * 3 * rtt * p_t +
                   p_endPos * p_t * p_t * p_t;
        }
        
		private static void Draw2DLineSegment(Vector2 p_point, Vector2 p_perpendicular) 
		{
			GL.TexCoord2 (0, 0);
			GL.Vertex (p_point - p_perpendicular);
			GL.TexCoord2 (0, 1);
			GL.Vertex (p_point + p_perpendicular);
		}
        
        private static void Draw3DLineSegment(Vector3 p_point, Vector3 p_perpendicular) 
        {
            GL.TexCoord2 (0, 0);
            GL.Vertex (p_point - p_perpendicular);
            GL.TexCoord2 (0, 1);
            GL.Vertex (p_point + p_perpendicular);
        }
        
		public static void DrawGUILine(Vector2 p_startPos, Vector2 p_endPos, Color p_color, Texture2D p_texture, float p_width = 1)
		{
			if (Event.current.type != EventType.Repaint)
				return;

            SetupLineMaterial(p_texture, p_color, false);
            GL.Begin(GL.TRIANGLE_STRIP);
            GL.Color(Color.white);

            Rect clippingRect = GetGUIClippingRect();
            clippingRect.x = clippingRect.y = 0;
            
            if (SegmentRectIntersection(clippingRect, ref p_startPos, ref p_endPos))
			{
				Vector2 perpWidthOffset = CalculateLinePerpendicular(p_startPos, p_endPos) * p_width / 2;
				Draw2DLineSegment (p_startPos, perpWidthOffset);
				Draw2DLineSegment (p_endPos, perpWidthOffset);
			}
            
            GL.End();
        }
        
        public static void Draw3DLine(Vector3 p_startPos, Vector3 p_endPos, Camera p_camera, float p_width, Color p_color, Texture2D p_texture = null)
        {
            SetupLineMaterial(p_texture, p_color, false);
            GL.Begin(GL.TRIANGLE_STRIP);
            GL.Color(Color.white);
            var perpendicular1 = Vector3.Cross(p_endPos - p_startPos, -p_camera.transform.forward);
            var distance1 = p_camera.transform.position - p_startPos;
            var size1 = p_width * distance1.magnitude / 100f;
            var perpendicular2 = Vector3.Cross(p_startPos - p_endPos, p_camera.transform.forward);
            var distance2 = p_camera.transform.position - p_endPos;
            var size2 = p_width * distance2.magnitude / 100f;

            Draw3DLineSegment(p_startPos, perpendicular1 * size1 / 2);
            Draw3DLineSegment(p_endPos, perpendicular2 * size2 / 2);

            GL.End();
        }

        private static bool SegmentRectIntersection(Rect p_rect, ref Vector2 p_point1, ref Vector2 p_point2)
        {
            bool point1Clipped;
            bool point2Clipped;
			return SegmentRectIntersection(p_rect, ref p_point1, ref p_point2, out point1Clipped, out point2Clipped);
		}
        
		private static bool SegmentRectIntersection(Rect p_rect, ref Vector2 p_point1, ref Vector2 p_point2, out bool p_point1Clipped, out bool p_point2Clipped)
		{
			float t0 = 0.0f;
			float t1 = 1.0f;
			float dx = p_point2.x - p_point1.x;
			float dy = p_point2.y - p_point1.y;

			if (ClipTest(-dx, p_point1.x - p_rect.xMin, ref t0, ref t1)) 
			{
				if (ClipTest(dx, p_rect.xMax - p_point1.x, ref t0, ref t1)) 
				{
					if (ClipTest(-dy, p_point1.y - p_rect.yMin, ref t0, ref t1)) 
					{
						if (ClipTest(dy, p_rect.yMax - p_point1.y, ref t0, ref t1)) 
						{
							p_point1Clipped = t0 > 0;
							p_point2Clipped = t1 < 1;

							if (p_point2Clipped)
							{
								p_point2.x = p_point1.x + t1 * dx;
								p_point2.y = p_point1.y + t1 * dy;
							}

							if (p_point1Clipped)
							{
								p_point1.x += t0 * dx;
								p_point1.y += t0 * dy;
							}

							return true;
						}
					}
				}
			}

			p_point2Clipped = p_point1Clipped = true;
			return false;
		}
        
		private static bool ClipTest(float p_p, float p_q, ref float p_t0, ref float p_t1)
		{
			float u = p_q / p_p;

			if (p_p < 0.0f)
			{
				if (u > p_t1)
					return false;
				if (u > p_t0)
					p_t0 = u;
			}
			else if (p_p > 0.0f)
			{
				if (u < p_t0)
					return false;
				if (u < p_t1)
					p_t1 = u;
			}
			else if (p_q < 0.0f)
				return false;

			return true;
		}
    }
}