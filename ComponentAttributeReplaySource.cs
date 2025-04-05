using System.Collections.Generic;
using UnityEngine;

public abstract class ComponentAttributeReplaySource : MonoBehaviour
{
    [SerializeField]
    protected AttributeReplaySourceSettings _settings;
    
    public bool IsPlaying { get; private set; }
    public bool IsPaused { get; private set; }

    public float CurrentTime { get; protected set; } = 0f;
    
    public abstract int LoadedFrameCount { get; }
    public abstract bool IsFinished { get; }

    public AttributeReplaySourceSettings CurrentReplaySettings { get; protected set; }
    public GameObjectReplaySource ParentReplaySource { get; protected set; }

    private void Update()
    {
        if (!IsPlaying || LoadedFrameCount == 0) 
            return;
        
        CurrentTime += Time.deltaTime;

        if (ParentReplaySource != null)
            return;
        
        UpdatePlayback();
        DetermineReplayLifetime();
    }

    public void Load(GameObjectReplaySource parent)
    {
        ParentReplaySource = parent;
        Load(ParentReplaySource.CurrentReplaySettings);
    }

    public abstract void UpdatePlayback();

    public abstract void Load(AttributeReplaySourceSettings settings = null);
    public abstract void Load(string directoryPath, AttributeReplaySourceSettings settings = null);
    public abstract void SetTime(float time);

    public abstract void OnLoopPlayback();

    public void Play()
    {
        if (LoadedFrameCount == 0)
        {
            Debug.LogWarning("[ReplaySource] No frames loaded to replay!");
            return;
        }
        
        OnPlaybackStart(CurrentReplaySettings);

        IsPlaying = true;
    }

    public void Stop()
    {
        OnPlaybackStop();
        ResetReplayValues();
        
        ParentReplaySource = null;
        IsPlaying = false;
        IsPaused = false;
    }

    public void Pause()
    {
        OnPlaybackPause();
        
        IsPlaying = false;
        IsPaused = true;
    }

    public void Continue()
    {
        OnPlaybackContinue();
        
        IsPlaying = true;
        IsPaused = false;
    }

    protected virtual void OnPlaybackStart(AttributeReplaySourceSettings settings) { }
    protected virtual void OnPlaybackStop() { }
    protected virtual void OnPlaybackPause() { }
    protected virtual void OnPlaybackContinue() { }
    
    public virtual void ResetReplayValues()
    {
        CurrentTime = 0.0f;
    }

    private void DetermineReplayLifetime()
    {
        if (!IsFinished)
            return;
        
        if (CurrentReplaySettings.Loop)
        {
            Debug.Log("[ReplaySource] End of replay reached. Looping playback.");
            
            ResetReplayValues();
            OnLoopPlayback();
        }
        else
        {
            Debug.Log("[ReplaySource] End of replay reached. Stopping playback.");
            
            Stop();
        }
    }
}

public abstract class ComponentAttributeReplaySource<T> : ComponentAttributeReplaySource
{
    public override bool IsFinished => CurrentTime >= _dataSource.TotalRecordingTime;
    public override int LoadedFrameCount => _loadedFrames.Count;
    
    private readonly List<T> _loadedFrames = new List<T>();
    private int _currentFrameIndex = 0;

    private IComponentAttributeReplayDataSource<T> _dataSource;

    protected virtual void Awake()
    {
        _dataSource = GetComponent<IComponentAttributeReplayDataSource<T>>();
    }

    public override void UpdatePlayback()
    {
        int loadOffsetIndex = _currentFrameIndex + CurrentReplaySettings.FrameLoadOffset;
        if (loadOffsetIndex >= _loadedFrames.Count - 1 && loadOffsetIndex < _dataSource.TotalRecordedFrames - 1)
            AddFramesAtFrameIndex(loadOffsetIndex);
        
        float frameInTime = Mathf.Clamp(CurrentTime, 0, _dataSource.TotalRecordingTime) / _dataSource.RecordingInterval;
        int currentFrameIndex = Mathf.Min(Mathf.FloorToInt(frameInTime), _loadedFrames.Count - 1);
        int nextFrameIndex = Mathf.Min(currentFrameIndex + 1, _loadedFrames.Count - 1);
        float time = frameInTime - currentFrameIndex;
        
        T currentFrame = _loadedFrames[currentFrameIndex];
        T nextFrame = _loadedFrames[nextFrameIndex];
        
        OnPlaybackUpdate(currentFrame, nextFrame, time);

        _currentFrameIndex = currentFrameIndex;
    }

    public override void Load(AttributeReplaySourceSettings settings = null)
    {
        if (_dataSource == null)
        {
            Debug.LogError("[ReplaySource] No data source assigned to replay source!");
            return;
        }
        
        Debug.Log("[ReplaySource] Loading frames from last recording.");
        
        CurrentReplaySettings = settings ?? _settings ?? AttributeReplaySourceSettings.Default;
        
        ResetReplayValues();
        LoadFramesAtFrameIndex(_currentFrameIndex);
    }

    public override void Load(string directoryPath, AttributeReplaySourceSettings settings = null)
    {
        if (_dataSource == null)
        {
            Debug.LogError("[ReplaySource] No data source assigned to replay source!");
            return;
        }
        
        Debug.Log($"[ReplaySource] Loading frames from directory: {directoryPath}");
        
        CurrentReplaySettings = settings ?? _settings ?? AttributeReplaySourceSettings.Default;
        
        ResetReplayValues();
        LoadFramesFromDirectory(directoryPath);
    }

    public override void OnLoopPlayback()
    {
        LoadFramesAtFrameIndex(_currentFrameIndex);
    }

    public override void SetTime(float time)
    {
        CurrentTime = Mathf.Clamp(time, 0f, _dataSource.TotalRecordingTime);
        _currentFrameIndex = Mathf.FloorToInt(CurrentTime / _dataSource.RecordingInterval);
        LoadFramesAtFrameIndex(_currentFrameIndex);
    }

    public override void ResetReplayValues()
    {
        base.ResetReplayValues();
        
        _currentFrameIndex = 0;
    }
    
    protected abstract void OnPlaybackUpdate(T currentFrame, T nextFrame, float time);

    private void LoadFramesFromDirectory(string directoryPath)
    {
        _loadedFrames.Clear();
        
        IReadOnlyList<T> frames = _dataSource.LoadFramesFromDirectory(directoryPath, CurrentReplaySettings.BufferSize);
        _loadedFrames.AddRange(frames);
    }
    
    private void LoadFramesAtFrameIndex(int frameIndex)
    {
        Debug.Log($"[ReplaySource] Loading frames from frame index: {frameIndex}");
        
        _loadedFrames.Clear();
        
        AddFramesAtFrameIndex(frameIndex);
    }

    private void AddFramesAtFrameIndex(int frameIndex)
    {
        IReadOnlyList<T> frames = _dataSource.LoadFramesFromIndex(frameIndex, CurrentReplaySettings.BufferSize);
        _loadedFrames.AddRange(frames);
    }
}
