using UnityEngine;
using System;
using System.IO;
public class GameObjectRecorder : MonoBehaviour, IComponentAttributeReplayDataSource
{
    [SerializeField]
    private AttributeRecorderSettings _settings;

    [SerializeField]
    private ComponentAttributeRecorder[] _recorders;

    public int TotalRecordedFrames { get; private set; }
    public float TotalRecordingTime { get; private set; }
    public float RecordingInterval => CurrentRecordingSettings.RecordingInterval;
    public bool IsRecording { get; private set; }
    public AttributeRecorderSettings CurrentRecordingSettings { get; private set; }
    private float _nextRecordingTime;

    private void Start()
    {
        CurrentRecordingSettings = _settings ?? AttributeRecorderSettings.Default;
    }

    private void Update()
    {
        if (!IsRecording || Time.time < _nextRecordingTime)
            return;
        
        foreach (ComponentAttributeRecorder recorder in _recorders)
            recorder.OnRecordingUpdate(CurrentRecordingSettings);

        TotalRecordingTime += RecordingInterval;
        TotalRecordedFrames++;
            
        _nextRecordingTime = Time.time + CurrentRecordingSettings.RecordingInterval;
    }
    
    public void StartRecording()
    {
        foreach (ComponentAttributeRecorder recorder in _recorders)
            recorder.StartRecording(this);

        IsRecording = true;
    }

    public void StopRecording()
    {
        foreach (ComponentAttributeRecorder recorder in _recorders)
            recorder.StopRecording();

        IsRecording = false;
    }

    public void DeleteAllRecordingFiles()
    {
        if (IsRecording)
        {
            Debug.LogError("Cannot delete recording files while recording is in progress!");
            return;
        }

        try
        {
            string identifier = GetComponent<IDistinguishable>()?.Identifier ?? gameObject.name;
            string[] replayDirectories = Directory.GetDirectories(Application.persistentDataPath, $"recording_{identifier}*");;
            foreach (string directory in replayDirectories)
                Directory.Delete(directory, true);
            
            Debug.Log($"Successfully deleted all recording files for {gameObject.name}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error deleting recording files: {e.Message}");
        }

        foreach (ComponentAttributeRecorder recorder in _recorders)
            recorder.ResetRecordingValues();
    }
}
