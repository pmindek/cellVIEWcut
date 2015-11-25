using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.UI;
using System.Collections.Generic;
using System;

namespace UIWidgets
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SpinnerFloat), true)]
	public class SpinnerFloatEditor : SelectableEditor
	{
		Dictionary<string,SerializedProperty> serializedProperties = new Dictionary<string,SerializedProperty>();
		
		protected string[] properties = new string[]{
			//InputField
			"m_TextComponent",
			"m_CaretBlinkRate",
			"m_SelectionColor",
			"m_HideMobileInput",
			"m_Placeholder",
			"m_OnValueChange",
			"m_EndEdit",
			
			//Spinner
			"_min",
			"_max",
			"_step",
			"_value",

			"Validation",

			"format",
			"_plusButton",
			"_minusButton",
			"HoldStartDelay",
			"HoldChangeDelay",

			"onValueChangeFloat",
			"onPlusClick",
			"onMinusClick",
		};
		
		protected override void OnEnable()
		{
			base.OnEnable();
			
			Array.ForEach(properties, x => {
				serializedProperties.Add(x, serializedObject.FindProperty(x));
			});
		}
		
		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			
			base.OnInspectorGUI();
			
			EditorGUILayout.Space();
			
			EditorGUILayout.PropertyField(serializedProperties["_min"]);
			EditorGUILayout.PropertyField(serializedProperties["_max"]);
			EditorGUILayout.PropertyField(serializedProperties["_step"]);
			EditorGUILayout.PropertyField(serializedProperties["_value"]);
			EditorGUILayout.PropertyField(serializedProperties["Validation"]);
			EditorGUILayout.PropertyField(serializedProperties["format"]);
			EditorGUILayout.PropertyField(serializedProperties["HoldStartDelay"]);
			EditorGUILayout.PropertyField(serializedProperties["HoldChangeDelay"]);
			EditorGUILayout.PropertyField(serializedProperties["_plusButton"]);
			EditorGUILayout.PropertyField(serializedProperties["_minusButton"]);
			
			EditorGUILayout.PropertyField(serializedProperties["m_TextComponent"]);
			
			if (serializedProperties["m_TextComponent"] != null && serializedProperties["m_TextComponent"].objectReferenceValue != null)
			{
				Text text = serializedProperties["m_TextComponent"].objectReferenceValue as Text;
				if (text.supportRichText)
				{
					EditorGUILayout.HelpBox("Using Rich Text with input is unsupported.", MessageType.Warning);
				}
				
				if (text.alignment != TextAnchor.UpperLeft &&
				    text.alignment != TextAnchor.UpperCenter &&
				    text.alignment != TextAnchor.UpperRight)
				{
					EditorGUILayout.HelpBox("Using a non upper alignment with input is unsupported.", MessageType.Warning);
				}
			}
			
			EditorGUI.BeginDisabledGroup(serializedProperties["m_TextComponent"] == null || serializedProperties["m_TextComponent"].objectReferenceValue == null);
			
			EditorGUILayout.Space();
			
			EditorGUILayout.PropertyField(serializedProperties["m_Placeholder"]);
			EditorGUILayout.PropertyField(serializedProperties["m_CaretBlinkRate"]);
			EditorGUILayout.PropertyField(serializedProperties["m_SelectionColor"]);
			EditorGUILayout.PropertyField(serializedProperties["m_HideMobileInput"]);
			
			EditorGUILayout.Space();
			
			EditorGUILayout.PropertyField(serializedProperties["m_OnValueChange"]);
			EditorGUILayout.PropertyField(serializedProperties["m_EndEdit"]);

			EditorGUILayout.PropertyField(serializedProperties["onValueChangeFloat"]);
			EditorGUILayout.PropertyField(serializedProperties["onPlusClick"]);
			EditorGUILayout.PropertyField(serializedProperties["onMinusClick"]);
			
			EditorGUI.EndDisabledGroup();
			
			serializedObject.ApplyModifiedProperties();
		}
	}
}