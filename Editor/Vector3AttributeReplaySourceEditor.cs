using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

[CustomEditor(typeof(Vector3AttributeReplaySource), true)]
public class Vector3AttributeReplaySourceEditor : UnityEditor.Editor
{
    private Vector3AttributeReplaySource replaySource;
    private Vector3AttributeRecorder recorder;
    private Vector2 replayFilesScrollPosition;
    private bool showReplayFiles = true;
    private Dictionary<string, bool> replayFoldouts = new Dictionary<string, bool>();

    private void OnEnable()
    {
        replaySource = (Vector3AttributeReplaySource)target;
        recorder = replaySource.GetComponent<Vector3AttributeRecorder>();
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        EditorGUILayout.Space(10);

        DrawReplayFilesSection();

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Playback controls are only available in play mode", MessageType.Info);
            return;
        }

        DrawPlaybackControls();
    }

    private void DrawReplayFilesSection()
    {
        EditorGUILayout.LabelField("Available Recordings", EditorStyles.boldLabel);
        showReplayFiles = EditorGUILayout.Foldout(showReplayFiles, "Recordings Overview");
        
        if (!showReplayFiles) return;

        string[] replayDirectories = AttributeRecorderSettings.GetReplayDirectoriesForGameObject(recorder.gameObject);
        if (replayDirectories.Length == 0)
        {
            EditorGUILayout.HelpBox("No recordings found. Use TransformRecorder to create new recordings.", MessageType.Info);
            return;
        }

        replayFilesScrollPosition = EditorGUILayout.BeginScrollView(replayFilesScrollPosition, EditorStyles.helpBox, GUILayout.MaxHeight(200));

        foreach (string directory in replayDirectories)
            DrawReplayDirectory(directory);

        EditorGUILayout.EndScrollView();
    }

    private void DrawReplayDirectory(string directory)
    {
        string directoryName = Path.GetFileName(directory);
        replayFoldouts.TryAdd(directoryName, false);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        EditorGUILayout.BeginHorizontal();
        replayFoldouts[directoryName] = EditorGUILayout.Foldout(replayFoldouts[directoryName], directoryName);

        GUI.enabled = Application.isPlaying && recorder != null && !recorder.IsRecording;
        if (GUILayout.Button("Play", GUILayout.Width(60)))
        {
            replaySource.Load(directory);
            replaySource.Play();
        }
        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();

        if (replayFoldouts[directoryName])
        {
            EditorGUI.indentLevel++;

            var files = Directory.GetFiles(directory, "positions_*.bin");
            long totalSize = 0;
            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                totalSize += fileInfo.Length;
                EditorGUILayout.LabelField($"File: {Path.GetFileName(file)}", $"Size: {GetFormattedSize(fileInfo.Length)}");
            }

            EditorGUILayout.LabelField("Total Files:", files.Length.ToString());
            EditorGUILayout.LabelField("Total Size:", GetFormattedSize(totalSize));
            EditorGUILayout.LabelField("Created:", File.GetCreationTime(directory).ToString());

            EditorGUI.indentLevel--;
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawPlaybackControls()
    {
        if (recorder != null && recorder.IsRecording)
        {
            EditorGUILayout.HelpBox("Cannot control playback while recording is in progress", MessageType.Warning);
            return;
        }

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Playback Controls", EditorStyles.boldLabel);

        DrawPlaybackStatus();
        EditorGUILayout.Space(5);
        DrawPlaybackButtons();
        DrawProgressBar();

        if (replaySource.IsPlaying)
        {
            Repaint();
        }
    }

    private void DrawPlaybackStatus()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUI.enabled = false;
        EditorGUILayout.Toggle("Is Playing", replaySource.IsPlaying);
        EditorGUILayout.FloatField("Current Time", replaySource.CurrentTime);
        EditorGUILayout.FloatField("Total Time", recorder.TotalRecordingTime);
        GUI.enabled = true;
        EditorGUILayout.EndVertical();
    }

    private void DrawPlaybackButtons()
    {
        EditorGUILayout.BeginHorizontal();
        
        if (!replaySource.IsPlaying)
        {
            if (GUILayout.Button("Play", GUILayout.Height(30)))
            {
                if (replaySource.IsPaused)
                {
                    replaySource.Continue();
                }
                else
                {
                    replaySource.Load();
                    replaySource.Play();
                }
            }
        }
        else
        {
            if (GUILayout.Button("Pause", GUILayout.Height(30)))
            {
                replaySource.Pause();
            }
        }

        GUI.enabled = replaySource.IsPlaying;
        if (GUILayout.Button("Stop", GUILayout.Height(30)))
        {
            replaySource.Stop();
        }
        GUI.enabled = true;
        
        EditorGUILayout.EndHorizontal();
    }

    private void DrawProgressBar()
    {
        if (recorder.TotalRecordingTime <= 0) return;

        Rect progressRect = EditorGUILayout.GetControlRect(GUILayout.Height(20));
        float progress = Mathf.Clamp01(replaySource.CurrentTime / recorder.TotalRecordingTime);
        EditorGUI.ProgressBar(progressRect, progress, $"Progress: {(progress * 100f):F1}%");
    }

    private string GetFormattedSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        int order = 0;
        double size = bytes;
        while (size >= 1024 && order < sizes.Length - 1)
        {
            order++;
            size /= 1024;
        }
        return $"{size:0.##} {sizes[order]}";
    }
} 