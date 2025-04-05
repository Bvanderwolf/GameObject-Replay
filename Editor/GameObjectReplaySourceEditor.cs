using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;

[CustomEditor(typeof(GameObjectReplaySource), true)]
public class GameObjectReplaySourceEditor : Editor
{
    private GameObjectReplaySource replaySource;
    public GameObjectRecorder recorder;
    private bool[] replaySourceFoldouts;
    private float timeSliderValue;
    private SerializedProperty replaySourcesProperty;
    private Vector2 replayFilesScrollPosition;
    private bool showReplayFiles = true;
    private Dictionary<string, bool> replayFoldouts = new Dictionary<string, bool>();

    private void OnEnable()
    {
        replaySource = (GameObjectReplaySource)target;
        recorder = replaySource.GetComponent<GameObjectRecorder>();
        replaySourcesProperty = serializedObject.FindProperty("_replaySources");
        UpdateReplaySourceFoldouts();
        timeSliderValue = 0f;
    }

    private void UpdateReplaySourceFoldouts()
    {
        replaySourceFoldouts = new bool[replaySourcesProperty.arraySize];
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        EditorGUILayout.Space(10);

        DrawReplayFilesSection();

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Replay controls are only available in play mode", MessageType.Info);
            return;
        }

        DrawReplayControls();
        DrawReplaySourceStatuses();
    }

    private void DrawReplayFilesSection()
    {
        EditorGUILayout.LabelField("Available Recordings", EditorStyles.boldLabel);
        showReplayFiles = EditorGUILayout.Foldout(showReplayFiles, "Recordings Overview");
        
        if (!showReplayFiles) return;

        string[] replayDirectories = AttributeRecorderSettings.GetReplayDirectoriesForGameObject(replaySource.gameObject);
        if (replayDirectories.Length == 0)
        {
            EditorGUILayout.HelpBox("No recordings found. Use GameObjectRecorder to create new recordings.", MessageType.Info);
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

        GUI.enabled = Application.isPlaying && !replaySource.IsPlaying;
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

            var files = Directory.GetFiles(directory, "*.bin");
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

    private void DrawReplayControls()
    {
        EditorGUILayout.LabelField("Replay Controls", EditorStyles.boldLabel);

        DrawReplayStatus();
        EditorGUILayout.Space(5);
        DrawReplayButtons();
        DrawProgressBar();

        if (replaySource.IsPlaying)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.HelpBox("Replay in progress...", MessageType.Info);
            Repaint();
        }
    }

    private void DrawReplayStatus()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUI.enabled = false;
        EditorGUILayout.Toggle("Is Playing", replaySource.IsPlaying);
        EditorGUILayout.Toggle("Is Paused", replaySource.IsPaused);
        GUI.enabled = true;
        EditorGUILayout.EndVertical();
    }

    private void DrawReplayButtons()
    {
        EditorGUILayout.BeginHorizontal();
        
        if (!replaySource.IsPlaying && !replaySource.IsPaused)
        {
            if (GUILayout.Button("Play", GUILayout.Height(30)))
            {
                replaySource.Load();
                replaySource.Play();
            }
        }
        else if (replaySource.IsPlaying)
        {
            if (GUILayout.Button("Pause", GUILayout.Height(30)))
            {
                replaySource.Pause();
            }
            if (GUILayout.Button("Stop", GUILayout.Height(30)))
            {
                replaySource.Stop();
            }
        }
        else if (replaySource.IsPaused)
        {
            if (GUILayout.Button("Continue", GUILayout.Height(30)))
            {
                replaySource.Continue();
            }
            if (GUILayout.Button("Stop", GUILayout.Height(30)))
            {
                replaySource.Stop();
            }
        }
        
        EditorGUILayout.EndHorizontal();
    }
    
    private void DrawProgressBar()
    {
        if (recorder.TotalRecordingTime <= 0) return;

        Rect progressRect = EditorGUILayout.GetControlRect(GUILayout.Height(20));
        float progress = Mathf.Clamp01(replaySource.CurrentTime / recorder.TotalRecordingTime);
        EditorGUI.ProgressBar(progressRect, progress, $"Progress: {(progress * 100f):F1}%");
    }

    private void DrawReplaySourceStatuses()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Component Replay Source Status", EditorStyles.boldLabel);

        if (replaySourcesProperty.arraySize != replaySourceFoldouts.Length)
        {
            UpdateReplaySourceFoldouts();
        }

        for (int i = 0; i < replaySourcesProperty.arraySize; i++)
        {
            var replaySourceProperty = replaySourcesProperty.GetArrayElementAtIndex(i);
            var componentReplaySource = replaySourceProperty.objectReferenceValue as ComponentAttributeReplaySource;
            
            if (componentReplaySource == null) continue;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            // Draw the foldout header with replay source name and status
            string status = componentReplaySource.IsPlaying ? "Playing" : 
                           componentReplaySource.IsPaused ? "Paused" : "Stopped";
            replaySourceFoldouts[i] = EditorGUILayout.Foldout(replaySourceFoldouts[i], 
                $"{componentReplaySource.GetType().Name} - {status}", 
                true);

            if (replaySourceFoldouts[i])
            {
                EditorGUI.indentLevel++;
                
                // Draw basic replay status
                GUI.enabled = false;
                EditorGUILayout.Toggle("Is Playing", componentReplaySource.IsPlaying);
                EditorGUILayout.Toggle("Is Paused", componentReplaySource.IsPaused);
                GUI.enabled = true;

                // Draw additional information for IComponentAttributeReplayDataSource
                if (componentReplaySource is IComponentAttributeReplayDataSource dataSource)
                {
                    EditorGUILayout.Space(5);
                    EditorGUILayout.LabelField("Replay Statistics", EditorStyles.boldLabel);
                    
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