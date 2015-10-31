// Decompiled with JetBrains decompiler
// Type: UnityEditor.ShadowCascadeSplitGUI
// Assembly: UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: B9F7FAEF-92F3-46FC-B88F-3451A2830B26
// Assembly location: D:\UnityProjects\pmindek\trunk\Library\UnityAssemblies\UnityEditor.dll

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class MultiRangeSlider
{
    private static readonly Color[] kCascadeColors = new Color[4]
    {
      new Color(0.5f, 0.5f, 0.6f, 1f),
      new Color(0.5f, 0.6f, 0.5f, 1f),
      new Color(0.6f, 0.6f, 0.5f, 1f),
      new Color(0.6f, 0.5f, 0.5f, 1f)
    };
    private static readonly GUIStyle s_CascadeSliderBG = (GUIStyle)"LODSliderRange";
    private static readonly GUIStyle s_TextCenteredStyle = new GUIStyle(EditorStyles.whiteMiniLabel)
    {
        alignment = TextAnchor.MiddleCenter
    };
    private static readonly int s_CascadeSliderId = "s_CascadeSliderId".GetHashCode();
    private static DrawCameraMode s_OldSceneDrawMode = DrawCameraMode.Textured;
    private const int kSliderbarTopMargin = 2;
    private const int kSliderbarHeight = 24;
    private const int kSliderbarBottomMargin = 2;
    private const int kPartitionHandleWidth = 2;
    private const int kPartitionHandleExtraHitAreaWidth = 2;
    private static MultiRangeSlider.DragCache s_DragCache;
    private static SceneView s_RestoreSceneView;
    private static bool s_OldSceneLightingMode;

    public static bool mouseDragging = false;

    private static float lastP0;
    private static float lastP1;
    private static float n1;

    public static bool resetMouse = false;


    public static void updateMousePositionWhileDragging(float newP0, float newP1)
    {
        if (mouseDragging && MultiRangeSlider.s_DragCache != null)
        {
            if (MultiRangeSlider.s_DragCache.m_ActivePartition == 0)
            {
                MultiRangeSlider.s_DragCache.m_NormalizedPartitionSize = newP0;
            }
            else
            {
                MultiRangeSlider.s_DragCache.m_NormalizedPartitionSize = newP1;
            }
        }
    }


    public static void HandleCascadeSliderGUI(ref float[] normalizedCascadePartitions)
    {
        //GUILayout.Label("Cascade splits");

        lastP0 = normalizedCascadePartitions[0];
        lastP1 = normalizedCascadePartitions[1];

        Rect rect = GUILayoutUtility.GetRect(GUIContent.none, MultiRangeSlider.s_CascadeSliderBG, new GUILayoutOption[2]
        {
        GUILayout.Height(28f),
        GUILayout.ExpandWidth(true)
        });
        GUI.Box(rect, GUIContent.none);
        float x = rect.x;
        float y = rect.y + 2f;
        float num1 = rect.width - (float)(normalizedCascadePartitions.Length * 2);
        Color color = GUI.color;
        Color backgroundColor = GUI.backgroundColor;
        int index1 = -1;
        float[] numArray = new float[normalizedCascadePartitions.Length + 1];
        Array.Copy((Array)normalizedCascadePartitions, (Array)numArray, normalizedCascadePartitions.Length);
        numArray[numArray.Length - 1] = 1f - Enumerable.Sum((IEnumerable<float>)normalizedCascadePartitions);
        int controlId = GUIUtility.GetControlID(MultiRangeSlider.s_CascadeSliderId, FocusType.Passive);
        Event current = Event.current;
        int activePartition = -1;
        for (int index2 = 0; index2 < numArray.Length; ++index2)
        {
            float num2 = numArray[index2];
            index1 = (index1 + 1) % MultiRangeSlider.kCascadeColors.Length;
            GUI.backgroundColor = MultiRangeSlider.kCascadeColors[index1];
            float width = num1 * num2;
            Rect position1 = new Rect(x, y, width, 24f);
            GUI.Box(position1, GUIContent.none, MultiRangeSlider.s_CascadeSliderBG);
            float num3 = x + width;
            GUI.color = Color.white;
            Rect position2 = position1;
            string str = string.Format("{0}\n{1:F1}%", (object)index2, (object)(float)((double)num2 * 100.0));

            //GUI.Label(position2, GUIContent.Temp(str, str), MultiRangeSlider.s_TextCenteredStyle);
            GUI.Label(position2, str, MultiRangeSlider.s_TextCenteredStyle);

            if (index2 != numArray.Length - 1)
            {
                GUI.backgroundColor = Color.black;
                Rect position3 = position1;
                position3.x = num3;
                position3.width = 2f;
                GUI.Box(position3, GUIContent.none, MultiRangeSlider.s_CascadeSliderBG);
                Rect position4 = position3;
                position4.xMin -= 2f;
                position4.xMax += 2f;
                if (position4.Contains(current.mousePosition))
                    activePartition = index2;
                if (MultiRangeSlider.s_DragCache == null)
                    EditorGUIUtility.AddCursorRect(position4, MouseCursor.ResizeHorizontal, controlId);
                x = num3 + 2f;
            }
            else
                break;
        }
        GUI.color = color;
        GUI.backgroundColor = backgroundColor;
        switch (current.GetTypeForControl(controlId))
        {
            case EventType.MouseDown:
                mouseDragging = true;

                if (activePartition < 0)
                    break;
                MultiRangeSlider.s_DragCache = new MultiRangeSlider.DragCache(activePartition, normalizedCascadePartitions[activePartition], current.mousePosition);
                if (GUIUtility.hotControl == 0)
                    GUIUtility.hotControl = controlId;
                current.Use();
                if (!((UnityEngine.Object)MultiRangeSlider.s_RestoreSceneView == (UnityEngine.Object)null))
                    break;
                MultiRangeSlider.s_RestoreSceneView = SceneView.lastActiveSceneView;
                if (!((UnityEngine.Object)MultiRangeSlider.s_RestoreSceneView != (UnityEngine.Object)null))
                    break;
                MultiRangeSlider.s_OldSceneDrawMode = MultiRangeSlider.s_RestoreSceneView.renderMode;
                MultiRangeSlider.s_OldSceneLightingMode = MultiRangeSlider.s_RestoreSceneView.m_SceneLighting;
                MultiRangeSlider.s_RestoreSceneView.renderMode = DrawCameraMode.ShadowCascades;
                break;
            case EventType.MouseUp:
                mouseDragging = false;

                if (GUIUtility.hotControl == controlId)
                {
                    GUIUtility.hotControl = 0;
                    current.Use();
                }
                MultiRangeSlider.s_DragCache = (MultiRangeSlider.DragCache)null;
                if (!((UnityEngine.Object)MultiRangeSlider.s_RestoreSceneView != (UnityEngine.Object)null))
                    break;
                MultiRangeSlider.s_RestoreSceneView.renderMode = MultiRangeSlider.s_OldSceneDrawMode;
                MultiRangeSlider.s_RestoreSceneView.m_SceneLighting = MultiRangeSlider.s_OldSceneLightingMode;
                MultiRangeSlider.s_RestoreSceneView = (SceneView)null;
                break;
            case EventType.MouseDrag:
                mouseDragging = true;

                if (GUIUtility.hotControl != controlId)
                    break;
                float num4 = (current.mousePosition - MultiRangeSlider.s_DragCache.m_LastCachedMousePosition).x / num1;
                if ((double)numArray[MultiRangeSlider.s_DragCache.m_ActivePartition] + (double)num4 > 0.0 && (double)numArray[MultiRangeSlider.s_DragCache.m_ActivePartition + 1] - (double)num4 > 0.0)
                {
                    MultiRangeSlider.s_DragCache.m_NormalizedPartitionSize += num4;
                    normalizedCascadePartitions[MultiRangeSlider.s_DragCache.m_ActivePartition] = MultiRangeSlider.s_DragCache.m_NormalizedPartitionSize;
                    if (MultiRangeSlider.s_DragCache.m_ActivePartition < normalizedCascadePartitions.Length - 1)
                        normalizedCascadePartitions[MultiRangeSlider.s_DragCache.m_ActivePartition + 1] -= num4;
                }
                MultiRangeSlider.s_DragCache.m_LastCachedMousePosition = current.mousePosition;
                current.Use();
                break;
        }
    }

    private class DragCache
    {
        public int m_ActivePartition;
        public float m_NormalizedPartitionSize;
        public Vector2 m_LastCachedMousePosition;

        public DragCache(int activePartition, float normalizedPartitionSize, Vector2 currentMousePos)
        {
            this.m_ActivePartition = activePartition;
            this.m_NormalizedPartitionSize = normalizedPartitionSize;
            this.m_LastCachedMousePosition = currentMousePos;
        }
    }
}