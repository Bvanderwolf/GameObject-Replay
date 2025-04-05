using UnityEngine;

[CreateAssetMenu(menuName = "Replay/AttributeReplaySourceSettings", fileName = "AttributeReplaySourceSettings")]
public class AttributeReplaySourceSettings : ScriptableObject
{
    [Header("Settings")]
    [SerializeField, Tooltip("Whether to replay the recorded again after it has finished.")]
    private bool _loop = DEFAULT_LOOP;
    
    [SerializeField, Tooltip("Number of positions to load at once")]
    private int _bufferSize = DEFAULT_BUFFER_SIZE;
    
    [SerializeField, Tooltip("Number of frames to load ahead")]
    private int _frameLoadOffset = DEFAULT_FRAME_LOAD_OFFSET;
    
    public bool Loop => _loop;
    public int BufferSize => _bufferSize;
    public int FrameLoadOffset => _frameLoadOffset;
    
    public const bool DEFAULT_LOOP = false;
    public const int DEFAULT_BUFFER_SIZE = 1000;
    public const int DEFAULT_FRAME_LOAD_OFFSET = 60;
    
    public static AttributeReplaySourceSettings Default => CreateInstance<AttributeReplaySourceSettings>();
}
