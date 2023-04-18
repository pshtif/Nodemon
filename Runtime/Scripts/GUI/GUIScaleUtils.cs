/*
 * Original from Seneral at https://github.com/Seneral/Node_Editor_Framework/blob/develop/Node_Editor_Framework/Runtime/Utilities/GUI/GUIScaleUtility.cs
 *
 * Modified by Peter @sHTiF Stefcek
 */

using UnityEngine;
using System.Collections.Generic;
using System;
using System.Reflection;

namespace Nodemon
{

	public static class GUIScaleUtils
	{
		private const int HEADER_SIZE = 23;
        
		private static bool _compabilityMode;
		private static bool _initiated;
        
		private static FieldInfo _currentGUILayoutCache;
		private static FieldInfo _currentTopLevelGroup;
        
		private static Func<Rect> _getTopRectDelegate;
		private static Func<Rect> _topmostRectDelegate;
        
		public static Rect TopRect
		{
			get
			{
				if (_getTopRectDelegate == null) 
					Init();
				return (Rect) _getTopRectDelegate.Invoke();
			}
		}

        public static Rect TopRectScreenSpace => _topmostRectDelegate.Invoke();
        
		public static List<Rect> currentRectStack { get; private set; }
		private static List<List<Rect>> _rectStackGroups;

		// Matrices stack
		private static List<Matrix4x4> _guiMatrices;
		private static List<bool> _adjustedGUILayout;

		private static bool _isEditorWindow;

        public static float lastZoom = 1;

        public static Vector2 CurrentScale =>
            new Vector2(1 / GUI.matrix.GetColumn(0).magnitude, 1 / GUI.matrix.GetColumn(1).magnitude);

        public static void CheckInit() 
		{
			if (!_initiated)
				Init ();
		}

		public static void Init() 
		{
			// As we can call Begin/Ends inside another, we need to save their states hierarchial in Lists (not Stack, as we need to iterate over them!):
			currentRectStack = new List<Rect> ();
			_rectStackGroups = new List<List<Rect>> ();
			_guiMatrices = new List<Matrix4x4> ();
			_adjustedGUILayout = new List<bool> ();
            
			// Fetch rect acessors using Reflection
			Assembly UnityEngine = Assembly.GetAssembly(typeof (UnityEngine.GUI));
			Type GUIClipType = UnityEngine.GetType("UnityEngine.GUIClip", true);

            PropertyInfo topmostRect = GUIClipType.GetProperty("topmostRect",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            
            MethodInfo GetTopmostRect = topmostRect != null
                ? (topmostRect.GetGetMethod(false) ?? topmostRect.GetGetMethod(true))
                : null;
            
            MethodInfo GetTopRect = GUIClipType.GetMethod("GetTopRect",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            
            MethodInfo ClipRect = GUIClipType.GetMethod("Clip",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, Type.DefaultBinder,
                new Type[] { typeof(Rect) }, new ParameterModifier[] { });

			if (GUIClipType == null || topmostRect == null || GetTopRect == null || ClipRect == null) 
			{
				// Debug.LogWarning ("GUIScaleUtility cannot run on this system! Compability mode enabled. For you that means you're not able to use the Node Editor inside more than one group:( Please PM me (Seneral @UnityForums) so I can figure out what causes this! Thanks!");
				// Debug.LogWarning ((GUIClipType == null? "GUIClipType is Null, " : "") + (topmostRect == null? "topmostRect is Null, " : "") + (GetTopRect == null? "GetTopRect is Null, " : "") + (ClipRect == null? "ClipRect is Null, " : ""));
				_compabilityMode = true;
				_initiated = true;
				return;
			}

			// Create simple acessor delegates
			_getTopRectDelegate = (Func<Rect>)Delegate.CreateDelegate (typeof(Func<Rect>), GetTopRect);
			_topmostRectDelegate = (Func<Rect>)Delegate.CreateDelegate (typeof(Func<Rect>), GetTopmostRect);

			if (_getTopRectDelegate == null || _topmostRectDelegate == null)
			{
				// Debug.LogWarning ("GUIScaleUtility cannot run on this system! Compability mode enabled. For you that means you're not able to use the Node Editor inside more than one group:( Please PM me (Seneral @UnityForums) so I can figure out what causes this! Thanks!");
				// Debug.LogWarning ((GUIClipType == null? "GUIClipType is Null, " : "") + (topmostRect == null? "topmostRect is Null, " : "") + (GetTopRect == null? "GetTopRect is Null, " : "") + (ClipRect == null? "ClipRect is Null, " : ""));
				_compabilityMode = true;
				_initiated = true;
				return;
			}

			// Sometimes, strange errors pop up (related to Mac?), which we try to catch and enable a compability Mode no supporting zooming in groups
			/*try
			{
				topmostRectDelegate.Invoke ();
			}
			catch (Exception e)
			{
				Debug.LogWarning ("GUIScaleUtility cannot run on this system! Compability mode enabled. For you that means you're not able to use the Node Editor inside more than one group:( Please PM me (Seneral @UnityForums) so I can figure out what causes this! Thanks!");
				Debug.Log (e.Message);
				compabilityMode = true;
			}*/
		
			_initiated = true;
		}
        
		public static Vector2 BeginScale(ref Rect p_rect, Vector2 p_zoomPivot, float p_zoom, bool p_isEditorWindow, bool p_adjustGUILayout) 
		{
			_isEditorWindow = p_isEditorWindow;
            lastZoom = p_zoom;

			Rect screenRect;
			if (_compabilityMode) 
			{ // In compability mode, we will assume only one top group and do everything manually, not using reflected calls (-> practically blind)
				GUI.EndGroup ();
				screenRect = p_rect;
				#if UNITY_EDITOR
				if (_isEditorWindow)
					screenRect.y += HEADER_SIZE;
				#endif
			}
			else
			{ // If it's supported, we take the completely generic way using reflected calls
				GUIScaleUtils.BeginNoClip();
				screenRect = GUIScaleUtils.GUIToScaledSpace(p_rect);
			}

			p_rect = Scale (screenRect, screenRect.position + p_zoomPivot, new Vector2 (p_zoom, p_zoom));

			// Now continue drawing using the new clipping group
			GUI.BeginGroup(p_rect);
			p_rect.position = Vector2.zero; // Adjust because we entered the new group

			// Because I currently found no way to actually scale to a custom pivot rather than (0, 0),
			// we'll make use of a cheat and just offset it accordingly to let it appear as if it would scroll to the center
			// Note, due to that, controls not adjusted are still scaled to (0, 0)
			Vector2 zoomPosAdjust = p_rect.center - screenRect.size/2 + p_zoomPivot;

			// For GUILayout, we can make this adjustment here if desired
			_adjustedGUILayout.Add(p_adjustGUILayout);
			if (p_adjustGUILayout)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Space(p_rect.center.x - screenRect.size.x + p_zoomPivot.x);
				GUILayout.BeginVertical();
				GUILayout.Space(p_rect.center.y - screenRect.size.y + p_zoomPivot.y);
			}

			// Take a matrix backup to restore back later on
			_guiMatrices.Add (GUI.matrix);

			// Scale GUI.matrix. After that we have the correct clipping group again.
			GUIUtility.ScaleAroundPivot(new Vector2 (1/p_zoom, 1/p_zoom), zoomPosAdjust);

			return zoomPosAdjust;
		}
        
		public static void EndScale() 
		{
			// Set last matrix and clipping group
			if (_guiMatrices.Count == 0 || _adjustedGUILayout.Count == 0)
				throw new UnityException ("GUIScaleUtility: You are ending more scale regions than you are beginning!");
			GUI.matrix = _guiMatrices[_guiMatrices.Count-1];
			_guiMatrices.RemoveAt (_guiMatrices.Count-1);

			// End GUILayout zoomPosAdjustment
			if (_adjustedGUILayout[_adjustedGUILayout.Count-1])
			{
				GUILayout.EndVertical ();
				GUILayout.EndHorizontal ();
			}
			_adjustedGUILayout.RemoveAt (_adjustedGUILayout.Count-1);

			// End the scaled group
			GUI.EndGroup ();

			if (_compabilityMode)
			{ // In compability mode, we don't know the previous group rect, but as we cannot use top groups there either way, we restore the screen group
				if (_isEditorWindow) // We're in an editor window
					GUI.BeginClip (new Rect (0, HEADER_SIZE, Screen.width, Screen.height-HEADER_SIZE));
				else
					GUI.BeginClip (new Rect (0, 0, Screen.width, Screen.height));
			}
			else
			{ // Else, restore the clips (groups)
				GUIScaleUtils.RestoreClips ();
			}
		}
        
		public static void BeginNoClip() 
		{
			// Record and close all clips one by one, from bottom to top, until we hit the 'origin'
			List<Rect> rectStackGroup = new List<Rect> ();
			Rect topMostClip = TopRect;
			while (topMostClip != new Rect (-10000, -10000, 40000, 40000)) 
			{
				rectStackGroup.Add (topMostClip);
				GUI.EndClip ();
				topMostClip = TopRect;
			}
			// Store the clips appropriately
			rectStackGroup.Reverse ();
			_rectStackGroups.Add (rectStackGroup);
			currentRectStack.AddRange (rectStackGroup);
		}
        
		public static void MoveClipsUp(int p_count) 
		{
			// Record and close all clips one by one, from bottom to top, until reached the count or hit the 'origin'
			List<Rect> rectStackGroup = new List<Rect> ();
			Rect topMostClip = TopRect;
			while (topMostClip != new Rect (-10000, -10000, 40000, 40000) && p_count > 0)
			{
				rectStackGroup.Add (topMostClip);
				GUI.EndClip ();
				topMostClip = TopRect;
				p_count--;
			}
			// Store the clips appropriately
			rectStackGroup.Reverse ();
			_rectStackGroups.Add (rectStackGroup);
			currentRectStack.AddRange (rectStackGroup);
		}
        
		public static void RestoreClips() 
		{
			if (_rectStackGroups.Count == 0)
			{
				Debug.LogError ("GUIClipHierarchy: BeginNoClip/MoveClipsUp - RestoreClips count not balanced!");
				return;
			}

			// Read and restore clips one by one, from top to bottom
			List<Rect> rectStackGroup = _rectStackGroups[_rectStackGroups.Count-1];
			for (int clipCnt = 0; clipCnt < rectStackGroup.Count; clipCnt++)
			{
				GUI.BeginClip (rectStackGroup[clipCnt]);
				currentRectStack.RemoveAt (currentRectStack.Count-1);
			}
			_rectStackGroups.RemoveAt (_rectStackGroups.Count-1);
		}
        
		public static void BeginNewLayout() 
		{
			if (_compabilityMode)
				return;
			// Will mimic a new layout by creating a new group at (0, 0). Will be restored though after ending the new Layout
			Rect topMostClip = TopRect;
			if (topMostClip != new Rect (-10000, -10000, 40000, 40000))
				GUILayout.BeginArea (new Rect (0, 0, topMostClip.width, topMostClip.height));
			else
				GUILayout.BeginArea (new Rect (0, 0, Screen.width, Screen.height));
		}
        
		public static void EndNewLayout () 
		{
			if (!_compabilityMode)
				GUILayout.EndArea ();
		}
        
		public static void BeginIgnoreMatrix() 
		{
			// Store and clean current matrix
			_guiMatrices.Add (GUI.matrix);
			GUI.matrix = Matrix4x4.identity;
		}
        
		public static void EndIgnoreMatrix() 
		{
			if (_guiMatrices.Count == 0)
				throw new UnityException ("GUIScaleutility: You are ending more ignoreMatrices than you are beginning!");
			// Read and assign previous matrix
			GUI.matrix = _guiMatrices[_guiMatrices.Count-1];
			_guiMatrices.RemoveAt (_guiMatrices.Count-1);
		}
        
		public static Vector2 Scale(Vector2 p_position, Vector2 p_pivot, Vector2 p_scale) 
		{
			return Vector2.Scale (p_position - p_pivot, p_scale) + p_pivot;
		}
        
		public static Rect Scale(Rect p_rect, Vector2 p_pivot, Vector2 p_scale) 
		{
			p_rect.position = Vector2.Scale (p_rect.position - p_pivot, p_scale) + p_pivot;
			p_rect.size = Vector2.Scale (p_rect.size, p_scale);
			return p_rect;
		}
        
		public static Vector2 ScaledToGUISpace(Vector2 p_scaledPosition) 
		{
			if (_rectStackGroups == null || _rectStackGroups.Count == 0)
				return p_scaledPosition;
			// Iterate through the clips and substract positions
			List<Rect> rectStackGroup = _rectStackGroups[_rectStackGroups.Count-1];
			for (int clipCnt = 0; clipCnt < rectStackGroup.Count; clipCnt++)
				p_scaledPosition -= rectStackGroup[clipCnt].position;
			return p_scaledPosition;
		}

		public static Rect ScaledToGUISpace(Rect p_scaledRect) 
		{
			if (_rectStackGroups == null || _rectStackGroups.Count == 0)
				return p_scaledRect;
			p_scaledRect.position = ScaledToGUISpace (p_scaledRect.position);
			return p_scaledRect;
		}
        
		public static Vector2 GUIToScaledSpace(Vector2 p_guiPosition) 
		{
			if (_rectStackGroups == null || _rectStackGroups.Count == 0)
				return p_guiPosition;
			// Iterate through the clips and add positions ontop
			List<Rect> rectStackGroup = _rectStackGroups[_rectStackGroups.Count-1];
			for (int clipCnt = 0; clipCnt < rectStackGroup.Count; clipCnt++)
				p_guiPosition += rectStackGroup[clipCnt].position;
			return p_guiPosition;
		}

		public static Rect GUIToScaledSpace(Rect p_rect) 
		{
			if (_rectStackGroups == null || _rectStackGroups.Count == 0)
				return p_rect;
			p_rect.position = GUIToScaledSpace (p_rect.position);
			return p_rect;
		}
        
		public static Vector2 GUIToScreenSpace(Vector2 p_position) 
		{
			#if UNITY_EDITOR
			if (_isEditorWindow)
				return p_position + TopRectScreenSpace.position - new Vector2 (0, HEADER_SIZE-1);
			#endif
			return p_position + TopRectScreenSpace.position;
		}
        
		public static Rect GUIToScreenSpace (Rect p_rect) 
		{
			p_rect.position += TopRectScreenSpace.position;
			#if UNITY_EDITOR
			if (_isEditorWindow)
				p_rect.y -= HEADER_SIZE-1;
			#endif
			return p_rect;
		}
    }
}
