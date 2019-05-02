using UnityEditor;
using UnityEngine;

namespace Utils.Views.Editor
{
	[CustomEditor(typeof(SmoothLayoutGroup)), CanEditMultipleObjects]
	public class SmoothLayoutGroupEditor : UnityEditor.Editor
	{
		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUILayout.PropertyField(serializedObject.FindProperty("_strict"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_isVertical"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Padding"), true);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_spacing"), true);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ChildAlignment"), true);

			Rect rect = EditorGUILayout.GetControlRect();
			rect = EditorGUI.PrefixLabel(rect, -1, new GUIContent("Child Controls Size"));
			rect.width = Mathf.Max(50, (rect.width - 4) / 3);
			EditorGUIUtility.labelWidth = 50;
			ToggleLeft(rect, serializedObject.FindProperty("_childControlWidth"), new GUIContent("Width"));
			rect.x += rect.width + 2;
			ToggleLeft(rect, serializedObject.FindProperty("_childControlHeight"), new GUIContent("Height"));
			EditorGUIUtility.labelWidth = 0;

			rect = EditorGUILayout.GetControlRect();
			rect = EditorGUI.PrefixLabel(rect, -1, new GUIContent("Child Force Expand"));
			rect.width = Mathf.Max(50, (rect.width - 4) / 3);
			EditorGUIUtility.labelWidth = 50;
			ToggleLeft(rect, serializedObject.FindProperty("_childForceExpandWidth"), new GUIContent("Width"));
			rect.x += rect.width + 2;
			ToggleLeft(rect, serializedObject.FindProperty("_childForceExpandHeight"), new GUIContent("Height"));
			EditorGUIUtility.labelWidth = 0;
		
			EditorGUILayout.Space();

			EditorGUILayout.PropertyField(serializedObject.FindProperty("_lazyDuration"));

			serializedObject.ApplyModifiedProperties();
		}

		void ToggleLeft(Rect position, SerializedProperty property, GUIContent label)
		{
			bool toggle = property.boolValue;
			EditorGUI.showMixedValue = property.hasMultipleDifferentValues;
			EditorGUI.BeginChangeCheck();

			int oldIndent = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;
			EditorGUI.ToggleLeft(position, label, toggle);
			EditorGUI.indentLevel = oldIndent;

			if (EditorGUI.EndChangeCheck())
			{
				property.boolValue = property.hasMultipleDifferentValues || !property.boolValue;
			}

			EditorGUI.showMixedValue = false;
		}
	}
}