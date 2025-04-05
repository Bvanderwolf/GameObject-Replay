using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Vector3AttributeRecorder), true)]
public class Vector3AttributeRecorderEditor : Editor
{
    private Vector3AttributeRecorder recorder;

    private void OnEnable()
    {
        recorder = (Vector3AttributeRecorder)target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (recorder.ParentRecorder != null)
        {
            EditorGUILayout.HelpBox($"This recorder is driven by the parent recorder: {recorder.ParentRecorder.name}", MessageType.Info);
            return;
        }

        EditorGUILayout.Space(10);

        DrawDeleteSection();

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Recording controls are only available in play mode", MessageType.Info);
            return;
        }

        DrawRecordingControls();
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
        EditorGUILayout.FloatField("Total Recording Time", recorder.TotalRecordingTime);
        EditorGUILayout.IntField("Total Recorded Positions", recorder.TotalRecordedFrames);
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
} 