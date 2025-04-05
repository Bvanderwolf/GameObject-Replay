using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

public abstract class ComponentAttributeRecorder<T> : ComponentAttributeRecorder, IComponentAttributeReplayDataSource<T>
{
    public int TotalRecordedFrames { get; protected set; }
    public float TotalRecordingTime { get; protected set;  }
    public string CurrentRecordingDirectory { get; protected set;  }
    public int CurrentFileIndex { get; private set;  }
    public float RecordingInterval => CurrentRecordingSettings.RecordingInterval;

    private readonly List<T> _loadedFrames = new List<T>();

    protected List<T> GetLoadedFrames() => _loadedFrames;

    protected override void OnRecordingStart(AttributeRecorderSettings settings)
    {
        string baseRecordingDirectoryPath = Application.persistentDataPath;
        CreateCurrentRecordingDirectory(baseRecordingDirectoryPath);
        ResetRecordingValues();
    }

    protected override void OnRecordingStart(GameObjectRecorder parent)
    {
        string baseRecordingDirectoryPath = AttributeRecorderSettings.GetRecordingDirectoryForGameObject(parent.gameObject);
        CreateCurrentRecordingDirectory(baseRecordingDirectoryPath);
        ResetRecordingValues();
    }

    public override void OnRecordingUpdate(AttributeRecorderSettings settings)
    {
        RecordNewFrame(_loadedFrames);
        
        if (_loadedFrames.Count >= CurrentRecordingSettings.MaxFramesInMemory)
            WriteRecordedFramesToFile();
        
        TotalRecordingTime += RecordingInterval;
        TotalRecordedFrames++;
    }

    protected override void OnRecordingStop(AttributeRecorderSettings settings)
    {
        if (CurrentRecordingSettings.SaveOnStop && _loadedFrames.Count > 0)
            WriteRecordedFramesToFile();
    }
    
    public void WriteRecordedFramesToFile()
    {
        Debug.Log($"[Recorder] Writing {_loadedFrames.Count} frames to file: {CurrentFileIndex}");
        
        WriteFramesToFile(_loadedFrames, CurrentFileIndex);
        
        _loadedFrames.Clear();

        CurrentFileIndex++;
    }
    
    public void DeleteAllRecordingFiles()
    {
        if (IsRecording)
        {
            Debug.LogError("[Recorder] Cannot delete recording files while recording is in progress!");
            return;
        }

        try
        {
            string identifier = GetComponent<IDistinguishable>()?.Identifier ?? gameObject.name;
            string[] replayDirectories = Directory.GetDirectories(Application.persistentDataPath, $"recording_{identifier}*");;
            foreach (string directory in replayDirectories)
                Directory.Delete(directory, true);
            
            Debug.Log($"[Recorder] Successfully deleted all recording files for {gameObject.name}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[Recorder] Error deleting recording files: {e.Message}");
        }

        ResetRecordingValues();
    }

    protected abstract void RecordNewFrame(List<T> frames);
    protected abstract void WriteFramesToFile(List<T> frames, int fileIndex);

    public abstract IReadOnlyList<T> LoadFramesFromIndex(int startIndex, int count);
    public abstract IReadOnlyList<T> LoadFramesFromDirectory(string directoryPath, int count);

    public override void ResetRecordingValues()
    {
        TotalRecordedFrames = 0;
        TotalRecordingTime = 0.0f;
        
        _loadedFrames.Clear();
    }

    private void CreateCurrentRecordingDirectory(string basePath)
    {
        CurrentRecordingDirectory = AttributeRecorderSettings.GetRecordingDirectoryForGameObject(gameObject, basePath);
        
        Directory.CreateDirectory(CurrentRecordingDirectory);

        Debug.Log($"[Recorder] Created recording directory: {CurrentRecordingDirectory}");
    }
}

public abstract class ComponentAttributeRecorder : MonoBehaviour
{
    [SerializeField]
    private AttributeRecorderSettings _settings;
    
    public bool IsRecording { get; private set; }
    public GameObjectRecorder ParentRecorder { get; private set; }

    public AttributeRecorderSettings CurrentRecordingSettings => _currentRecordingSettings ?? _settings ?? AttributeRecorderSettings.Default;

    private float _nextRecordingTime;
    private AttributeRecorderSettings _currentRecordingSettings;

    private void Update()
    {
        if (!IsRecording || Time.time < _nextRecordingTime || ParentRecorder != null)
            return;

        OnRecordingUpdate(_currentRecordingSettings);
            
        _nextRecordingTime = Time.time + CurrentRecordingSettings.RecordingInterval;
    }
    
    public void StartRecording(GameObjectRecorder parent)
    {
        if (IsRecording)
        {
            Debug.LogWarning("Already recording! Stop current recording before starting a new one.");
            return;
        }
        
        ParentRecorder = parent;
        
        _currentRecordingSettings = ParentRecorder.CurrentRecordingSettings;
        OnRecordingStart(parent);
        
        IsRecording = true;
    }

    public void StartRecording(AttributeRecorderSettings settings = null)
    {
        if (IsRecording)
        {
            Debug.LogWarning("Already recording! Stop current recording before starting a new one.");
            return;
        }

        _currentRecordingSettings = settings ?? _settings ?? AttributeRecorderSettings.Default;
        OnRecordingStart(_currentRecordingSettings);
        
        IsRecording = true;
    }

    public void StopRecording()
    {
        OnRecordingStop(_currentRecordingSettings);
        
        ParentRecorder = null;
        IsRecording = false;
    }

    protected abstract void OnRecordingStart(AttributeRecorderSettings settings);
    protected abstract void OnRecordingStart(GameObjectRecorder parent);
    public abstract void OnRecordingUpdate(AttributeRecorderSettings settings);
    protected abstract void OnRecordingStop(AttributeRecorderSettings settings);
    public abstract void ResetRecordingValues();
}