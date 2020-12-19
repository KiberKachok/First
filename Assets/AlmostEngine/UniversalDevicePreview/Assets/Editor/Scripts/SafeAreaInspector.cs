using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace AlmostEngine.Preview
{
	[CustomEditor (typeof(SafeArea))]
	[CanEditMultipleObjects]
	public class SafeAreaInspector : Editor
	{
		protected SafeArea m_SafeArea;


		void OnEnable ()
		{
			m_SafeArea = (SafeArea)target;
		}

		public override void OnInspectorGUI ()
		{
			serializedObject.Update ();

            if (m_SafeArea.GetComponent<RectTransform>() == null || m_SafeArea.transform.parent == null)
            {
                EditorGUILayout.HelpBox("Safe area must be placed on a valid UI object.", MessageType.Error);
                return;
            }

            m_SafeArea.m_Panel = m_SafeArea.GetComponent<RectTransform>();

			if (m_SafeArea.gameObject.activeInHierarchy
			    && m_SafeArea.m_HorizontalConstraintType != SafeArea.Constraint.NONE
			    && m_SafeArea.m_Panel.parent.GetComponent<Canvas> () == null
			    && (m_SafeArea.m_Panel.parent.GetComponentInParent<RectTransform> ().anchorMin.x != 0f
			    || m_SafeArea.m_Panel.parent.GetComponentInParent<RectTransform> ().anchorMax.x != 1f
			    || m_SafeArea.m_Panel.parent.GetComponentInParent<RectTransform> ().offsetMin.x != 0f
			    || m_SafeArea.m_Panel.parent.GetComponentInParent<RectTransform> ().offsetMax.x != 0f)) {

				EditorGUILayout.HelpBox ("Horizontal Safe Area must be placed within fully horizontally stretched parent to be scaled properly. " +
				"\nParent anchor min.x: " + m_SafeArea.m_Panel.parent.GetComponentInParent<RectTransform> ().anchorMin.x + " should be 0." +
				"\nParent anchor max.x: " + m_SafeArea.m_Panel.parent.GetComponentInParent<RectTransform> ().anchorMax.x + " should be 1." +
				"\nParent anchor offsetMin.x: " + m_SafeArea.m_Panel.parent.GetComponentInParent<RectTransform> ().offsetMin.x + " should be 0." +
				"\nParent anchor offsetMax.x: " + m_SafeArea.m_Panel.parent.GetComponentInParent<RectTransform> ().offsetMax.x + " should be 0.", 
					MessageType.Error);

			}
			if (m_SafeArea.gameObject.activeInHierarchy
			    && m_SafeArea.m_VerticalConstraintType != SafeArea.Constraint.NONE
			    && m_SafeArea.m_Panel.parent.GetComponent<Canvas> () == null
			    && (m_SafeArea.m_Panel.parent.GetComponentInParent<RectTransform> ().anchorMin.y != 0f
			    || m_SafeArea.m_Panel.parent.GetComponentInParent<RectTransform> ().anchorMax.y != 1f
			    || m_SafeArea.m_Panel.parent.GetComponentInParent<RectTransform> ().offsetMin.y != 0f
			    || m_SafeArea.m_Panel.parent.GetComponentInParent<RectTransform> ().offsetMax.y != 0f)) {


				EditorGUILayout.HelpBox ("Vertical Safe Area must be placed within fully vertical stretched parent to be scaled properly. " +
				"\nParent anchor min.y: " + m_SafeArea.m_Panel.parent.GetComponentInParent<RectTransform> ().anchorMin.y + " should be 0." +
				"\nParent anchor max.y: " + m_SafeArea.m_Panel.parent.GetComponentInParent<RectTransform> ().anchorMax.y + " should be 1." +
				"\nParent anchor offsetMin.y: " + m_SafeArea.m_Panel.parent.GetComponentInParent<RectTransform> ().offsetMin.y + " should be 0." +
				"\nParent anchor offsetMax.y: " + m_SafeArea.m_Panel.parent.GetComponentInParent<RectTransform> ().offsetMax.y + " should be 0.", 
					MessageType.Error);
			}




			EditorGUILayout.PropertyField (serializedObject.FindProperty ("m_HorizontalConstraintType"));
			if (m_SafeArea.m_HorizontalConstraintType != SafeArea.Constraint.NONE) {
				EditorGUILayout.PropertyField (serializedObject.FindProperty ("m_HorizontalConstraint"));
			}

			EditorGUILayout.Separator ();

			EditorGUILayout.PropertyField (serializedObject.FindProperty ("m_VerticalConstraintType"));
			if (m_SafeArea.m_VerticalConstraintType != SafeArea.Constraint.NONE) {
				EditorGUILayout.PropertyField (serializedObject.FindProperty ("m_VerticalConstraint"));
			}

			EditorGUILayout.Separator ();



//			EditorGUILayout.LabelField ("Default position " + m_SafeArea.m_DefaultAnchoredPosition.ToString ());
//			EditorGUILayout.LabelField ("Defaul size " + m_SafeArea.m_DefaultSize.ToString ());
//			EditorGUILayout.LabelField ("Defaul anchor min " + m_SafeArea.m_DefaultAnchorMin.ToString ());
//			EditorGUILayout.LabelField ("Defaul anchor max " + m_SafeArea.m_DefaultAnchorMax.ToString ());


			EditorGUILayout.Separator ();
			EditorGUILayout.Separator ();

			//			if (GUILayout.Button ("Set position & size as default")) {

			if (!Application.isPlaying
				&& (ResolutionGalleryWindow.m_Window == null || (!ResolutionGalleryWindow.m_Window.m_IsUpdating && !ResolutionGalleryWindow.m_Window.m_IsUpdating))
			    && (ResolutionPreviewWindow.m_Window == null || (!ResolutionPreviewWindow.m_Window.m_IsUpdating && !ResolutionPreviewWindow.m_Window.m_IsUpdating))) {
//				Vector2 pos = m_SafeArea.m_Panel.anchoredPosition;
//				if (pos != m_SafeArea.m_DefaultAnchoredPosition) {
//					m_SafeArea.m_DefaultAnchoredPosition = pos;
//					EditorUtility.SetDirty (m_SafeArea);
//				}
//
//				Vector2 size = m_SafeArea.m_Panel.sizeDelta;
//				if (size != m_SafeArea.m_DefaultSize) {
//					m_SafeArea.m_DefaultSize = size;
//					EditorUtility.SetDirty (m_SafeArea);
//				}

				m_SafeArea.m_DefaultAnchorMin = m_SafeArea.m_Panel.anchorMin;
				m_SafeArea.m_DefaultAnchorMax = m_SafeArea.m_Panel.anchorMax;
			}



			serializedObject.ApplyModifiedProperties ();
		}
	}
}