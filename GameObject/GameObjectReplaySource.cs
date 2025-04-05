using UnityEngine;

public class GameObjectReplaySource : MonoBehaviour
{
   [SerializeField]
   private AttributeReplaySourceSettings _settings;

   [SerializeField]
   private ComponentAttributeReplaySource[] _replaySources;
   
   public bool IsPlaying { get; private set; }
   public bool IsPaused { get; private set; }
   public float CurrentTime { get; private set; } = 0f;
   
   public AttributeReplaySourceSettings CurrentReplaySettings { get; private set; }

   private IComponentAttributeReplayDataSource _dataSource;

   private void Awake()
   {
      _dataSource = GetComponent<IComponentAttributeReplayDataSource>();
   }

   private void Start()
   {
      CurrentReplaySettings = _settings ?? AttributeReplaySourceSettings.Default;
   }

   private void Update()
   {
      if (!IsPlaying || _replaySources.Length == 0)
         return;
      
      CurrentTime += Time.deltaTime;
      
      foreach (ComponentAttributeReplaySource replaySource in _replaySources)
         replaySource.UpdatePlayback();
      
      DetermineReplayLifeTime();
   }

   private void DetermineReplayLifeTime()
   {
      if (CurrentTime < _dataSource.TotalRecordingTime)
         return;
        
      if (CurrentReplaySettings.Loop)
      {
         Debug.Log("[ReplaySource] End of replay reached. Looping playback.");

         foreach (ComponentAttributeReplaySource replaySource in _replaySources)
         {
            replaySource.ResetReplayValues();
            replaySource.OnLoopPlayback();
         }
      }
      else
      {
         Debug.Log("[ReplaySource] End of replay reached. Stopping playback.");
            
         Stop();
      }
   }

   public void Load()
   {
      foreach (ComponentAttributeReplaySource replaySource in _replaySources)
         replaySource.Load(this);
   }

   public void Load(string directoryPath)
   {
      string timestamp = directoryPath.Substring(directoryPath.LastIndexOf("_") + 1);
      AttributeReplaySourceSettings settings = CurrentReplaySettings;

      foreach (ComponentAttributeReplaySource replaySource in _replaySources)
      {
         string replaySourceDirectoryPath = AttributeRecorderSettings.GetRecordingDirectoryForGameObject(replaySource.gameObject, directoryPath, timestamp);
         replaySource.Load(replaySourceDirectoryPath, settings);
      }
   }

   public void Play()
   {
      foreach (ComponentAttributeReplaySource replaySource in _replaySources)
         replaySource.Play();
      
      IsPlaying = true;
   }
   
   public void Stop()
   {
      foreach (ComponentAttributeReplaySource replaySource in _replaySources)
         replaySource.Stop();
      
      IsPlaying = false;
      IsPaused = false;
   }
   
   public void Pause()
   {
      foreach (ComponentAttributeReplaySource replaySource in _replaySources)
         replaySource.Pause();
      
      IsPlaying = false;
      IsPaused = true;
   }
   
   public void Continue()
   {
      foreach (ComponentAttributeReplaySource replaySource in _replaySources)
         replaySource.Continue();
      
      IsPlaying = true;
      IsPaused = false;
   }

   public void SetTime(float time)
   {
      foreach (ComponentAttributeReplaySource replaySource in _replaySources)
         replaySource.SetTime(time);
   }
}
