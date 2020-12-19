using UnityEngine;
using UnityEngine.UI;

using AlmostEngine.Screenshot;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AlmostEngine.Preview
{
	[ExecuteInEditMode]
	public class SafeArea : MonoBehaviour
	{
		[HideInInspector]
		public RectTransform m_Panel;

		#region Constraints

		public enum HorizontalConstraint
		{
			LEFT,
			RIGHT,
			LEFT_AND_RIGHT}
		;

		public enum VerticalConstraint
		{
			UP,
			DOWN,
			UP_AND_DOWN}
		;


		public enum Constraint
		{
			NONE,
			SNAP,
			PUSH,
			ENLARGE}
		;

		public Constraint m_HorizontalConstraintType = Constraint.NONE;
		public HorizontalConstraint m_HorizontalConstraint = HorizontalConstraint.LEFT_AND_RIGHT;

		public Constraint m_VerticalConstraintType = Constraint.NONE;
		public VerticalConstraint m_VerticalConstraint = VerticalConstraint.UP_AND_DOWN;

		#endregion

		#region Handle Callbacks

		public Vector2 m_DefaultAnchorMin = new Vector2 (-99f, -99f);
		public Vector2 m_DefaultAnchorMax = new Vector2 (-99f, -99f);

		#endregion

		#region Start & update

		void Start ()
		{

			// PlayerSettings.Android.renderOutsideSafeArea = false;


			#if UNITY_EDITOR 
			if (!Application.isPlaying) {
				return;
			}
			#endif

			// Apply safe area at startup
			ApplySafeAreaIfNeeded ();
		}

		void Update ()
		{
			#if UNITY_EDITOR 
			// In editor (update execute in edit mode) we only apply the safe area when previewing a device
			if (!m_CanUpdateSafeArea) {
				return;
			}
			#endif

			// We constantly check if the safe area changed, for instance if the screen is rotated
			ApplySafeAreaIfNeeded ();
		}

		#endregion

		#region Safe Area & constraints

		Rect m_LastSafeArea = new Rect (0, 0, 0, 0);

		public void Restore ()
		{
			// Restore default anchor
			m_Panel.anchorMin = m_DefaultAnchorMin;
			m_Panel.anchorMax = m_DefaultAnchorMax;
		}

		public void ApplySafeArea (Rect rect)
		{
//			Debug.Log ("Apply " + name + " safe area " + rect);

			if (m_Panel == null || Application.isEditor) {
				m_Panel = GetComponent<RectTransform> ();
			}
			if (m_DefaultAnchorMin == new Vector2 (-99f, -99f)) {
				m_DefaultAnchorMin = m_Panel.anchorMin;
				m_DefaultAnchorMax = m_Panel.anchorMax;
			}

			#if UNITY_EDITOR
			if (!Application.isPlaying) {
				// Save current values to enable UNDO if something goes wrong
				Undo.RecordObject (m_Panel, "Safe Area");
			}
			#endif

			// Safe current safe area to apply the modification only when changed
			m_LastSafeArea = rect;

			// Restore default anchor
			m_Panel.anchorMin = m_DefaultAnchorMin;
			m_Panel.anchorMax = m_DefaultAnchorMax;

			// HORIZONTAL
			if (m_HorizontalConstraintType == Constraint.SNAP) {
				if (m_HorizontalConstraint == HorizontalConstraint.LEFT || m_HorizontalConstraint == HorizontalConstraint.LEFT_AND_RIGHT) {
					Vector2 anchorMin = m_Panel.anchorMin;
					anchorMin.x = rect.position.x / Screen.width;
					m_Panel.anchorMin = anchorMin;
				}
				if (m_HorizontalConstraint == HorizontalConstraint.RIGHT || m_HorizontalConstraint == HorizontalConstraint.LEFT_AND_RIGHT) {
					Vector2 anchorMax = m_Panel.anchorMax; 
					anchorMax.x = (rect.position.x + rect.size.x) / Screen.width;
					m_Panel.anchorMax = anchorMax;
				}
			}

			if (m_HorizontalConstraintType == Constraint.ENLARGE) {
				if (m_HorizontalConstraint == HorizontalConstraint.LEFT || m_HorizontalConstraint == HorizontalConstraint.LEFT_AND_RIGHT) {
					Vector2 anchorMax = m_Panel.anchorMax; 
					anchorMax.x = anchorMax.x + rect.position.x / Screen.width;
					m_Panel.anchorMax = anchorMax;
				}
				if (m_HorizontalConstraint == HorizontalConstraint.RIGHT || m_HorizontalConstraint == HorizontalConstraint.LEFT_AND_RIGHT) {
					Vector2 anchorMin = m_Panel.anchorMin; 
					anchorMin.x = anchorMin.x - (Screen.width - rect.width - rect.position.x) / Screen.width;
					m_Panel.anchorMin = anchorMin;
				}
			}


			if (m_HorizontalConstraintType == Constraint.PUSH) {
				if (m_HorizontalConstraint == HorizontalConstraint.LEFT || m_HorizontalConstraint == HorizontalConstraint.LEFT_AND_RIGHT) {
					Vector2 anchorMin = m_Panel.anchorMin;
					anchorMin.x = anchorMin.x + rect.position.x / Screen.width;
					m_Panel.anchorMin = anchorMin;
					Vector2 anchorMax = m_Panel.anchorMax; 
					anchorMax.x = anchorMax.x + rect.position.x / Screen.width;
					m_Panel.anchorMax = anchorMax;
				}
				if (m_HorizontalConstraint == HorizontalConstraint.RIGHT || m_HorizontalConstraint == HorizontalConstraint.LEFT_AND_RIGHT) {
					Vector2 anchorMin = m_Panel.anchorMin;
					anchorMin.x = anchorMin.x - (Screen.width - rect.width - rect.position.x) / Screen.width;
					m_Panel.anchorMin = anchorMin;
					Vector2 anchorMax = m_Panel.anchorMax; 
					anchorMax.x = anchorMax.x - (Screen.width - rect.width - rect.position.x) / Screen.width;
					m_Panel.anchorMax = anchorMax;
				}
			}


			// VERTICAL

			if (m_VerticalConstraintType == Constraint.SNAP) {
				if (m_VerticalConstraint == VerticalConstraint.DOWN || m_VerticalConstraint == VerticalConstraint.UP_AND_DOWN) {
					Vector2 anchorMin = m_Panel.anchorMin;
					anchorMin.y = rect.position.y / Screen.height;
					m_Panel.anchorMin = anchorMin;
				}
				if (m_VerticalConstraint == VerticalConstraint.UP || m_VerticalConstraint == VerticalConstraint.UP_AND_DOWN) {
					Vector2 anchorMax = m_Panel.anchorMax; 
					anchorMax.y = (rect.position.y + rect.size.y) / Screen.height;
					m_Panel.anchorMax = anchorMax;
				}
			}

			if (m_VerticalConstraintType == Constraint.ENLARGE) {
				if (m_VerticalConstraint == VerticalConstraint.UP || m_VerticalConstraint == VerticalConstraint.UP_AND_DOWN) {
					Vector2 anchorMin = m_Panel.anchorMin; 
					anchorMin.y = anchorMin.y - (Screen.height - rect.height - rect.position.y) / Screen.height;
					m_Panel.anchorMin = anchorMin;
				}
				if (m_VerticalConstraint == VerticalConstraint.DOWN || m_VerticalConstraint == VerticalConstraint.UP_AND_DOWN) {
					Vector2 anchorMax = m_Panel.anchorMax; 
					anchorMax.y = anchorMax.y + rect.position.y / Screen.height;
					m_Panel.anchorMax = anchorMax;
				}
			}

			if (m_VerticalConstraintType == Constraint.PUSH) {
				if (m_VerticalConstraint == VerticalConstraint.UP || m_VerticalConstraint == VerticalConstraint.UP_AND_DOWN) {
					Vector2 anchorMin = m_Panel.anchorMin;
					anchorMin.y = anchorMin.y - (Screen.height - rect.height - rect.position.y) / Screen.height;
					m_Panel.anchorMin = anchorMin;
					Vector2 anchorMax = m_Panel.anchorMax; 
					anchorMax.y = anchorMax.y - (Screen.height - rect.height - rect.position.y) / Screen.height;
					m_Panel.anchorMax = anchorMax;
				}
				if (m_VerticalConstraint == VerticalConstraint.DOWN || m_VerticalConstraint == VerticalConstraint.UP_AND_DOWN) {
					Vector2 anchorMin = m_Panel.anchorMin;
					anchorMin.y = anchorMin.y + rect.position.y / Screen.height;
					m_Panel.anchorMin = anchorMin;
					Vector2 anchorMax = m_Panel.anchorMax; 
					anchorMax.y = anchorMax.y + rect.position.y / Screen.height;
					m_Panel.anchorMax = anchorMax;
				}
			}

		}

		#endregion



		void ApplySafeAreaIfNeeded ()
		{

			Rect safeArea = DeviceInfo.GetSafeArea ();

			#if !UNITY_EDITOR 
			if (safeArea == m_LastSafeArea) {
				return;
			}
			#endif

			ApplySafeArea (safeArea);

		}

		#region Events and callbacks logic to update safe area in editor


		#if UNITY_EDITOR

		bool m_CanUpdateSafeArea = false;

		void OnEnable ()
		{
			ScreenshotTaker.onResolutionUpdateStartDelegate += DeviceStartUpdated;
			ScreenshotTaker.onResolutionUpdateEndDelegate += DeviceEndUpdated;
			ScreenshotTaker.onResolutionScreenResizedDelegate += DeviceResized;
		}

		void OnDisable ()
		{
			ScreenshotTaker.onResolutionUpdateStartDelegate -= DeviceStartUpdated;
			ScreenshotTaker.onResolutionUpdateEndDelegate -= DeviceEndUpdated;
			ScreenshotTaker.onResolutionScreenResizedDelegate -= DeviceResized;
		}

		void DeviceStartUpdated (ScreenshotResolution device)
		{
			// In live preview, we ignore the resize event for non device,
			// to prevent resizing to the device full preview with border 
			if (Application.isPlaying && device.m_ResolutionName == "" && DeviceInfo.m_IsLivePreview) {
				return;
			}
//			Debug.Log ("Start " + device.m_ResolutionName);

			// When a device simulation starts, we allow the safe area to be updated
			m_CanUpdateSafeArea = true;
		}

		void DeviceResized (ScreenshotResolution device)
		{
		}

		void DeviceEndUpdated (ScreenshotResolution device)
		{
			// When the resize is done we stop the update rights
			m_CanUpdateSafeArea = false;

			// We restore the safe area except if it is a live preview to prevent flickering
			if (!Application.isPlaying || !DeviceInfo.m_IsLivePreview) {
				Restore ();
			}
		}

		#endif

		#endregion

	}
}
