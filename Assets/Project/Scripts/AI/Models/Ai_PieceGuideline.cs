using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

[CreateAssetMenu(fileName = "IdealOrientation", menuName = "Miniclip/Ai/IdealOrientation")]
public class Ai_PieceGuideline : ScriptableObject
{
    public List<Orientation> orientations;
    public List<Ai_IdealTopology> idealTopologies;

    public List<Ai_TopologySequencePriority> GetTopologySequencesByPriority()
    {
        var sequencesToReturn = new List<Ai_TopologySequencePriority>();
        foreach (var orientation in orientations)
        {
            var priority = new Ai_TopologySequencePriority
            {
                orientation = orientation,
                topologySequences = GetTopologySequencesOfOrientation(orientation)
            };

            if (priority.topologySequences != null)
                sequencesToReturn.Add(priority);
        }

        return sequencesToReturn;
    }

    public List<Ai_TopologySequence> GetTopologySequencesOfOrientation(Orientation orientation)
    {
        if (!orientations.Contains(orientation)) return null;

        var indexOf = orientations.IndexOf(orientation);
        return (idealTopologies.Count <= indexOf) ? null : idealTopologies[indexOf].topologySequences;
    }
}

#region <--- EDITOR --->

#if UNITY_EDITOR
[CustomEditor(typeof(Ai_PieceGuideline))]
public class Ai_PieceGuidelineEditor : Editor
{
    private SerializedObject me;
    private SerializedProperty orientations;
    private SerializedProperty idealTopologies;
    private GUISkin buttonSkin;

    private void OnEnable()
    {
        me = new SerializedObject(target);

        orientations = me.FindProperty("orientations");
        idealTopologies = me.FindProperty("idealTopologies");

        buttonSkin = (GUISkin)AssetDatabase.LoadAssetAtPath("Assets/Project/Resources/GUISkins/Borderless.guiskin", 
            typeof(GUISkin));
    }

    public override void OnInspectorGUI()
    {
        me.Update();

        if (orientations.arraySize > 0)
        {
            for (int i = 0; i < orientations.arraySize; i++)
            {
                GUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    GUILayout.BeginHorizontal();
                    {
                        if (GUILayout.Button(EditorGUIUtility.IconContent("d_TreeEditor.Trash"), buttonSkin.button, GUILayout.ExpandWidth(false)))
                        {
                            orientations.DeleteArrayElementAtIndex(i);
                            idealTopologies.DeleteArrayElementAtIndex(i);
                            me.ApplyModifiedProperties();
                            return;
                        }
                        EditorGUILayout.PropertyField(orientations.GetArrayElementAtIndex(i), new GUIContent("Orientation " + i + ":"));
                        GUI.enabled = i - 1 >= 0;
                        if (GUILayout.Button(EditorGUIUtility.IconContent("CollabPush"), buttonSkin.button, GUILayout.ExpandWidth(false)))
                        {
                            orientations.MoveArrayElement(i, i - 1);
                            idealTopologies.MoveArrayElement(i, i - 1);
                            me.ApplyModifiedProperties();
                            return;
                        }

                        GUI.enabled = i + 1 < orientations.arraySize;
                        if (GUILayout.Button(EditorGUIUtility.IconContent("CollabPull"), buttonSkin.button, GUILayout.ExpandWidth(false)))
                        {
                            orientations.MoveArrayElement(i, i + 1);
                            idealTopologies.MoveArrayElement(i, i + 1);
                            me.ApplyModifiedProperties();
                            return;
                        }

                        GUI.enabled = true;
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.BeginVertical();
                    {
                        DrawSequences(i);
                    }
                    GUILayout.EndVertical();
                }
                GUILayout.EndVertical();
                EditorGUILayout.Space();
            }
        }

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        if (GUILayout.Button("Add Orientation"))
        {
            orientations.arraySize++;
            idealTopologies.arraySize++;
        }

        me.ApplyModifiedProperties();
    }

    private void DrawSequences(int i)
    {
        var topologySequences = idealTopologies.GetArrayElementAtIndex(i).FindPropertyRelative("topologySequences");
        if (topologySequences.arraySize > 0)
        {
            for (int j = 0; j < topologySequences.arraySize; j++)
            {
                var sequences = topologySequences.GetArrayElementAtIndex(j).FindPropertyRelative("sequence");
                if (sequences.arraySize <= 0)
                    sequences.arraySize = 1;

                GUILayout.BeginHorizontal(EditorStyles.helpBox);
                {
                    GUI.enabled = j - 1 >= 0;
                    if (GUILayout.Button(EditorGUIUtility.IconContent("CollabPush"), buttonSkin.button, GUILayout.ExpandWidth(false)))
                    {
                        topologySequences.MoveArrayElement(j, j - 1);
                        me.ApplyModifiedProperties();
                        return;
                    }

                    GUI.enabled = j + 1 < topologySequences.arraySize;
                    if (GUILayout.Button(EditorGUIUtility.IconContent("CollabPull"), buttonSkin.button, GUILayout.ExpandWidth(false)))
                    {
                        topologySequences.MoveArrayElement(j, j + 1);
                        me.ApplyModifiedProperties();
                        return;
                    }

                    GUI.enabled = true;
                    DrawSequence(j, sequences);

                    EditorGUILayout.Space();
                    if (GUILayout.Button(EditorGUIUtility.IconContent("d_TreeEditor.Trash"), buttonSkin.button, GUILayout.ExpandWidth(false)))
                    {
                        topologySequences.DeleteArrayElementAtIndex(j);
                        me.ApplyModifiedProperties();
                        return;
                    }
                }
                GUILayout.EndHorizontal();
            }
        }

        if (GUILayout.Button("Add Topology"))
        {
            topologySequences.arraySize++;
        }
    }

    private void DrawSequence(int index, SerializedProperty sequences)
    {
        GUILayout.BeginHorizontal();
        {
            GUILayout.Label("Priority " + index + ":", GUILayout.ExpandWidth(false));
            GUILayout.Label("", GUILayout.ExpandWidth(false));
            float originalValue = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 1;

            if(sequences.arraySize - 1 > 0)
            {
                if (GUILayout.Button(EditorGUIUtility.IconContent("d_Toolbar Minus"), buttonSkin.button, GUILayout.ExpandWidth(false)))
                    sequences.arraySize--;
            }

            for (int i = 0; i < sequences.arraySize; i++)
            {
                sequences.GetArrayElementAtIndex(i).intValue =
                    EditorGUILayout.IntField("", sequences.GetArrayElementAtIndex(i).intValue, GUILayout.ExpandWidth(false));
            }

            EditorGUIUtility.labelWidth = originalValue;

            if (sequences.arraySize + 1 <= 4)
            {
                if (GUILayout.Button(EditorGUIUtility.IconContent("d_Toolbar Plus"), buttonSkin.button, GUILayout.ExpandWidth(false)))
                    sequences.arraySize++;
            }            
        }
        GUILayout.EndHorizontal();
    }
}
#endif

#endregion <--- EDITOR --->

[System.Serializable]
public class Ai_IdealTopology
{
    public List<Ai_TopologySequence> topologySequences;
}

[System.Serializable]
public class Ai_TopologySequence
{
    public List<int> sequence;
}

[System.Serializable]
public class Ai_TopologySequencePriority
{
    public Orientation orientation;
    public List<Ai_TopologySequence> topologySequences;
}