using UnityEngine;

public class TransformPositionReplaySource : Vector3AttributeReplaySource
{
    protected override void PlayAttributeValue(Vector3 value)
    {
        transform.position = value;
    }
} 