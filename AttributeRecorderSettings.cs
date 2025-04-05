using System;
using System.IO;
using UnityEngine;

[CreateAssetMenu(menuName = "Replay/AttributeRecorderSettings", fileName = "AttributeRecorderSettings")]
public class AttributeRecorderSettings : ScriptableObject
{
    [Header("Settings")]
    [SerializeField, Tooltip("Time between recordings in seconds")]
    private float _recordingInterval = DEFAULT_RECORDING_INTERVAL;
    
    [SerializeField, Tooltip("The maximum number of frames each class can record in memory")]
    private int _maxFramesInMemory = DEFAULT_MAX_FRAMES_IN_MEMORY;
    
    [SerializeField, Tooltip("Whether to save the recording when the recording is stopped")]
    private bool _saveOnStop = DEFAULT_SAVE_ON_STOP;
    
    public float RecordingInterval => _recordingInterval;
    public int MaxFramesInMemory => _maxFramesInMemory;
    public bool SaveOnStop => _saveOnStop;
    
    public const float DEFAULT_RECORDING_INTERVAL = 0.1f;
    public const int DEFAULT_MAX_FRAMES_IN_MEMORY = 100000;
    public const bool DEFAULT_SAVE_ON_STOP = true;

    public static AttributeRecorderSettings Default => CreateInstance<AttributeRecorderSettings>();
    
    public static string GetRecordingDirectoryForGameObject(GameObject gameObject)
    {
        string basePath = Application.persistentDataPath;
        return GetRecordingDirectoryForGameObject(gameObject, basePath);
    }

    public static string GetRecordingDirectoryForGameObject(GameObject gameObject, string basePath)
    {
        string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        return (GetRecordingDirectoryForGameObject(gameObject, basePath, timestamp));
    }

    public static string GetRecordingDirectoryForGameObject(GameObject gameObject, string basePath, string timestamp)
    {
        string identifier = gameObject.GetComponent<IDistinguishable>()?.Identifier ?? gameObject.name;
        return (Path.Combine(basePath, $"recording_{identifier}_{timestamp}"));
    }

    public static string[] GetReplayDirectoriesForGameObject(GameObject gameObject)
    {
        string basePath = Application.persistentDataPath;
        return (GetReplayDirectoriesForGameObject(gameObject, basePath));
    }
    
    public static string[] GetReplayDirectoriesForGameObject(GameObject gameObject, string basePath)
    {
        string identifier = gameObject.GetComponent<IDistinguishable>()?.Identifier ?? gameObject.name;
        string searchPattern = $"recording_{identifier}*";
        return Directory.GetDirectories(basePath, searchPattern);
    }
}
