using UnityEngine;
using UnityEditor;
using System;

[CustomEditor(typeof(GameObjectRecorder), true)]
public class GameObjectRecorderEditor : Editor
{
    private GameObjectRecorder recorder;
    private bool[] recorderFoldouts;

    private void OnEnable()
    {
        recorder = (GameObjectRecorder)target;
        UpdateRecorderFoldouts();
    }

    private void UpdateRecorderFoldouts()
    {
        var serializedRecorders = serializedObject.FindProperty("_recorders");
        recorderFoldouts = new bool[serializedRecorders.arraySize];
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        EditorGUILayout.Space(10);

        DrawDeleteSection();

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Recording controls are only available in play mode", MessageType.Info);
            return;
        }

        DrawRecordingControls();
        DrawRecorderStatuses();
    }

    private void DrawDeleteSection()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.HelpBox("This will permanently delete all recording files for this object.", MessageType.Warning);

        Color originalColor = GUI.backgroundColor;
        GUI.backgroundColor = new Color(0.9f, 0.6f, 0.6f);

        if (GUILayout.Button("Delete All Recording Files", GUILayout.Height(25)))
        {
            if (EditorUtility.DisplayDialog("Delete Recording Files",
                $"Are you sure you want to delete all recording files for {recorder.gameObject.name}?",
                "Delete",
                "Cancel"))
            {
                recorder.DeleteAllRecordingFiles();
            }
        }

        GUI.backgroundColor = originalColor;
        EditorGUILayout.EndVertical();
    }

    private void DrawRecordingControls()
    {
        EditorGUILayout.LabelField("Recording Controls", EditorStyles.boldLabel);

        DrawRecordingStatus();
        EditorGUILayout.Space(5);
        DrawRecordingButtons();

        if (recorder.IsRecording)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.HelpBox("Recording in progress...", MessageType.Info);
            Repaint();
        }
    }

    private void DrawRecordingStatus()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUI.enabled = false;
        EditorGUILayout.Toggle("Is Recording", recorder.IsRecording);
        GUI.enabled = true;
        EditorGUILayout.EndVertical();
    }

    private void DrawRecordingButtons()
    {
        EditorGUILayout.BeginHorizontal();
        
        if (!recorder.IsRecording)
        {
            if (GUILayout.Button("Start Recording", GUILayout.Height(30)))
            {
                recorder.StartRecording();
            }
        }
        else
        {
            if (GUILayout.Button("Stop Recording", GUILayout.Height(30)))
            {
                recorder.StopRecording();
            }
        }
        
        EditorGUILayout.EndHorizontal();
    }

    private void DrawRecorderStatuses()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Component Recorder Status", EditorStyles.boldLabel);

        var serializedRecorders = serializedObject.FindProperty("_recorders");
        if (serializedRecorders.arraySize != recorderFoldouts.Length)
        {
            UpdateRecorderFoldouts();
        }

        for (int i = 0; i < serializedRecorders.arraySize; i++)
        {
            var recorderProperty = serializedRecorders.GetArrayElementAtIndex(i);
            var componentRecorder = recorderProperty.objectReferenceValue as ComponentAttributeRecorder;
            
            if (componentRecorder == null) continue;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            // Draw the foldout header with recorder name and status
            recorderFoldouts[i] = EditorGUILayout.Foldout(recorderFoldouts[i], 
                $"{componentRecorder.GetType().Name} - {(componentRecorder.IsRecording ? "Recording" : "Stopped")}", 
                true);

            if (recorderFoldouts[i])
            {
                EditorGUI.indentLevel++;
                
                // Draw basic recording status
                GUI.enabled = false;
                EditorGUILayout.Toggle("Is Recording", componentRecorder.IsRecording);
                GUI.enabled = true;

                // Draw additional information for IComponentAttributeReplayDataSource
                if (componentRecorder is IComponentAttributeReplayDataSource dataSource)
                {
                    EditorGUILayout.Space(5);
                    EditorGUILayout.LabelField("Recording Statistics", EditorStyles.boldLabel);
                    
                    GUI.enabled = false;
                    EditorGUILayout.IntField("Total Recorded Frames", dataSource.TotalRecordedFrames);
                    EditorGUILayout.FloatField("Total Recording Time", dataSource.TotalRecordingTime);
                    GUI.enabled = true;
                }
                
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.EndVertical();
        }
    }
} 