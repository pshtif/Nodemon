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
        
        private static Rect GetClippingRect()
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

        private static void SetupLineMaterial(Texture p_texture, Color p_color) 
        {
            if (_lineMaterial == null)
            {
                Shader lineShader = Shader.Find("Hidden/Nodemon/LineShader");
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

            _lineMaterial.SetTexture ("_LineTexture", p_texture);
            _lineMaterial.SetColor ("_LineColor", p_color);
            _lineMaterial.SetPass (0);
        }
        
		public static void DrawBezier(Vector2 p_startPos, Vector2 p_endPos, Vector2 p_startTan, Vector2 p_endTan, Color p_color, Texture2D p_texture, float p_width = 1)
		{
			if (Event.current.type != EventType.Repaint)
				return;
            
            Rect clippingRect = GetClippingRect();
            
			clippingRect.x = clippingRect.y = 0;
			Rect bounds = new Rect(Mathf.Min(p_startPos.x, p_endPos.x), Mathf.Min(p_startPos.y, p_endPos.y), 
								Mathf.Abs(p_startPos.x - p_endPos.x), Mathf.Abs(p_startPos.y - p_endPos.y));
            
			if (!bounds.Overlaps(clippingRect))
				return;
            
			int segmentCount = CalculateBezierSegmentCount(p_startPos, p_endPos, p_startTan, p_endTan);

			DrawBezier(p_startPos, p_endPos, p_startTan, p_endTan, p_color, p_texture, segmentCount, p_width);
		}
        
		public static void DrawBezier(Vector2 p_startPos, Vector2 p_endPos, Vector2 p_startTan, Vector2 p_endTan, Color p_color, Texture2D p_texture, int p_segmentCount, float p_width)
		{
			if (Event.current.type != EventType.Repaint && Event.current.type != EventType.KeyDown)
				return;

            Rect clippingRect = GetClippingRect();
            
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

            DrawPolygonLine(bezierPoints, p_color, p_texture, p_width);
		}
        
		public static void DrawPolygonLine(Vector2[] p_points, Color p_color, Texture2D p_texture, float p_width = 1)
		{
			if (Event.current.type != EventType.Repaint && Event.current.type != EventType.KeyDown)
				return;
            
			if (p_points.Length == 1)
				return;

            if (p_points.Length == 2)
            {
                DrawLine(p_points[0], p_points[1], p_color, p_texture, p_width);
            }

            SetupLineMaterial(p_texture, p_color);
			GL.Begin (GL.TRIANGLE_STRIP);
			GL.Color (Color.white);
            
            Rect clippingRect = GetClippingRect();
            
			clippingRect.x = clippingRect.y = 0;

			Vector2 curPoint = p_points[0], nextPoint, perpendicular;
			bool clippedP0, clippedP1;
			for (int pointCnt = 1; pointCnt < p_points.Length; pointCnt++) 
			{
				nextPoint = p_points[pointCnt];
                
				Vector2 curPointOriginal = curPoint, nextPointOriginal = nextPoint;
				if (SegmentRectIntersection(clippingRect, ref curPoint, ref nextPoint, out clippedP0, out clippedP1))
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

                    if (clippedP0)
					{ 
						//GL.End ();
						//GL.Begin (GL.TRIANGLE_STRIP);
						DrawLineSegment(curPoint, perpendicular * p_width/2);
					}

                    if (pointCnt == 1)
                    {
                        DrawLineSegment(curPoint, CalculateLinePerpendicular(curPoint, nextPoint) * p_width / 2);
                    }

                    DrawLineSegment(nextPoint, perpendicular * p_width/2);
				}
				else if (clippedP1)
				{ 
					//GL.End ();
					//GL.Begin(GL.TRIANGLE_STRIP);
				}
                
				curPoint = nextPointOriginal;
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

			return p_startPos  * rt*rt*rt + 
				p_startTan * 3 * rt * rtt + 
				p_endTan   * 3 * rtt * p_t + 
				p_endPos   * p_t*p_t*p_t;
		}
        
		private static void DrawLineSegment(Vector2 p_point, Vector2 p_perpendicular) 
		{
			GL.TexCoord2 (0, 0);
			GL.Vertex (p_point - p_perpendicular);
			GL.TexCoord2 (0, 1);
			GL.Vertex (p_point + p_perpendicular);
		}
        
		public static void DrawLine(Vector2 p_startPos, Vector2 p_endPos, Color p_color, Texture2D p_texture, float p_width = 1)
		{
			if (Event.current.type != EventType.Repaint)
				return;

			// Setup
			SetupLineMaterial (p_texture, p_color);
			GL.Begin (GL.TRIANGLE_STRIP);
			GL.Color (Color.white);
			// Fetch clipping rect
            
            Rect clippingRect = GetClippingRect();
            
			clippingRect.x = clippingRect.y = 0;
			// Clip to rect
			if (SegmentRectIntersection(clippingRect, ref p_startPos, ref p_endPos))
			{ // Draw with clipped line if it is visible
				Vector2 perpWidthOffset = CalculateLinePerpendicular (p_startPos, p_endPos) * p_width / 2;
				DrawLineSegment (p_startPos, perpWidthOffset);
				DrawLineSegment (p_endPos, perpWidthOffset);
			}
			// Finalize drawing
			GL.End ();
		}
        
		private static bool SegmentRectIntersection(Rect p_rect, ref Vector2 p_point1, ref Vector2 p_point2)
		{
			bool cP0, cP1;
			return SegmentRectIntersection (p_rect, ref p_point1, ref p_point2, out cP0, out cP1);
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