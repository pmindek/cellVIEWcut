using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Reflection;

namespace UIWidgets
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(ListViewBase), true)]
	public class ListViewCustomBaseEditor : Editor
	{
		protected bool IsListViewCustom = false;

		protected Dictionary<string,SerializedProperty> SerializedProperties = new Dictionary<string,SerializedProperty>();
		protected Dictionary<string,SerializedProperty> SerializedEvents = new Dictionary<string,SerializedProperty>();

		protected List<string> Properties = new List<string>{
			"customItems",
			"Multiple",
			"selectedIndex",
			
			"direction",

			"DefaultItem",
			"Container",
			"scrollRect",

			"defaultColor",
			"defaultBackgroundColor",

			"HighlightedColor",
			"HighlightedBackgroundColor",

			"selectedColor",
			"selectedBackgroundColor",

			//"OnSelectObject",

			"EndScrollDelay",
		};

		protected List<string> Events = new List<string>{
			"OnSelect",
			"OnDeselect",
			//"OnSubmit",
			//"OnCancel",
			//"OnItemSelect",
			//"OnItemCancel",
			//"OnFocusIn",
			//"OnFocusOut",
			"OnSelectObject",
			"OnDeselectObject",
			//"OnPointerEnterObject",
			//"OnPointerExitObject",
			"OnStartScrolling",
			"OnEndScrolling",
		};

		static bool DetectListViewCustom(object instance)
		{
			Type type = instance.GetType();
			while (type != null)
			{
				if (type.FullName.StartsWith("UIWidgets.ListViewCustom`2", StringComparison.InvariantCulture))
				{
					return true;
				}
				type = type.BaseType;
			}
			return false;
		}

		protected virtual void OnEnable()
		{
			if (!IsListViewCustom)
			{
				IsListViewCustom = DetectListViewCustom(serializedObject.targetObject);
			}

			if (IsListViewCustom)
			{
				Properties.ForEach(x => {
					SerializedProperties.Add(x, serializedObject.FindProperty(x));
				});
				Events.ForEach(x => {
					SerializedEvents.Add(x, serializedObject.FindProperty(x));
				});
			}
		}

		public bool ShowEvents;

		public override void OnInspectorGUI()
		{
			if (IsListViewCustom)
			{
				serializedObject.Update();

				SerializedProperties.ForEach(x => EditorGUILayout.PropertyField(x.Value, true));

				EditorGUILayout.BeginVertical();
				ShowEvents = GUILayout.Toggle(ShowEvents, "Events", "Foldout", GUILayout.ExpandWidth(true));
				if (ShowEvents)
				{
					SerializedEvents.ForEach(x => EditorGUILayout.PropertyField(x.Value, true));
				}
				EditorGUILayout.EndVertical();

				serializedObject.ApplyModifiedProperties();

				var showWarning = false;
				Array.ForEach(targets, x => {
					var ourType = x.GetType(); 
					
					var mi = ourType.GetMethod("CanOptimize", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
					
					if (mi!= null){
						var canOptimize = (Func<bool>)Delegate.CreateDelegate(typeof(Func<bool>), x, mi);
						showWarning |= !canOptimize.Invoke();
					}
				});
				if (showWarning)
				{
					EditorGUILayout.HelpBox("Optimization requires specified ScrollRect and Container should have EasyLayout component.", MessageType.Warning);
				}
			}
			else
			{
				DrawDefaultInspector();
			}
		}
	}
}